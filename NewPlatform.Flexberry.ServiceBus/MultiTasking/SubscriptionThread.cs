namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;
    using MessageSenders;

    /// <summary>
    /// Поток, запускающийся для конкретной подписки. Отслеживает новые сообщения и выполняет отправку клиентам.
    /// </summary>
    public class SubscriptionThread
    {
        /// <summary>
        /// Количество сообщений, которые вычитываются из БД за один раз для последующей отправки или при обновлении.
        /// </summary>
        private const int MessagesPerQuery = 10000;

        /// <summary>
        /// Объект для реализации пауз с возможностью пробуждения извне.
        /// </summary>
        private readonly ManualResetEvent waitScanDb4CallBackHandle;

        private readonly IStatisticsService _statistics;

        private readonly IDataService _dataService;

        private readonly ILogger _logger;

        /// <summary>
        /// LangDef, использующийся при создании ограничений.
        /// </summary>
        private readonly ExternalLangDef ldef;

        /// <summary>
        /// Словарь для хранения информации о сообщениях, которые находятся в отправке.
        /// Ключ - первичный ключ сообщения, значение - флаг, говорящий, выполняется ли в данный момент отправка сообщения.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, Task<bool>> sendingTasks = new ConcurrentDictionary<Guid, Task<bool>>();

        /// <summary>
        /// Объект для разделения доступа к свойству <see cref="IsActive"/> между потоками.
        /// </summary>
        private readonly object isActiveLockObject = new object();

        /// <summary>
        /// Закэшированное время, на которое будет засыпать поток.
        /// </summary>
        private int? sendingCallbackTimeout;

        /// <summary>
        /// Поле для кэширования значения свойства <see cref="ScanningTimeout"/>.
        /// </summary>
        private int? scanningTimeout;

        /// <summary>
        /// Поле для хранения значения свойств <see cref="IsActive"/>.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Подписка, используемая в данном потоке.
        /// </summary>
        public Subscription Subscription { get; set; }

        /// <summary>
        /// Выполняется ли в данный момент поток отправки.
        /// </summary>
        public bool IsExecuting
        {
            get
            {
                return ExecThread.IsAlive;
            }
        }

        /// <summary>
        /// Закэшированное время, на которое будет засыпать поток.
        /// </summary>
        public int SendingCallbackTimeOut
        {
            get
            {
                if (!sendingCallbackTimeout.HasValue)
                {
                    sendingCallbackTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["SendingCallbackTimeOut"]);
                }

                return sendingCallbackTimeout.Value;
            }
        }

        /// <summary>
        /// Период между сканированиями БД потоком в случае отсутствия сообщений.
        /// </summary>
        public int ScanningTimeout
        {
            get
            {
                if (!scanningTimeout.HasValue)
                    scanningTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ScanningTimeout"]);

                return scanningTimeout.Value;
            }
        }

        /// <summary>
        /// Поток, в котором будет производиться отправка сообщений клиентам.
        /// </summary>
        private Thread ExecThread { get; set; }

        /// <summary>
        /// Свойство, сообщающее потоку отправки, что пора завершать работу (если <c>false</c>).
        /// </summary>
        private bool IsActive
        {
            get
            {
                lock (isActiveLockObject)
                {
                    return isActive;
                }
            }

            set
            {
                lock (isActiveLockObject)
                {
                    isActive = value;
                }
            }
        }

        /// <summary>
        /// Получить экземпляр <see cref="LoadingCustomizationStruct" /> для загрузки подписок по идентификаторам клиента и типа сообщения.
        /// <para>В полученной структуре будет задана сортировка по приоритету (по убыванию) и по дате формирования (по возрастанию).</para>
        /// </summary>
        /// <param name="ldef">LanguageDef для формирования ограничения.</param>
        /// <param name="clientGuid">Идентификатор клиента.</param>
        /// <param name="messageTypeGuid">Идентификатор типа сообщений подписки.</param>
        /// <param name="messagesView">
        /// Представление, которое будет использовано в создаваемой LCS.
        /// По умолчанию используется представление <see cref="Сообщение.Views.SB_СообщениеE"/>.
        /// </param>
        /// <returns>
        /// Структура для загрузки подписок с помощью сервиса данных.
        /// </returns>
        public static LoadingCustomizationStruct GetSubscriptionLcs(ExternalLangDef ldef, Guid clientGuid, Guid messageTypeGuid, View messagesView = null)
        {
            if (messagesView == null)
                messagesView = Message.Views.MessageEditView;

            var lcsMessages = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), messagesView);

            lcsMessages.LimitFunction = ldef.GetFunction(
                ldef.funcAND,
                ldef.GetFunction(
                    ldef.funcEQ,
                    new VariableDef(ldef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)),
                    clientGuid),
                ldef.GetFunction(
                    ldef.funcEQ,
                    new VariableDef(ldef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)),
                    messageTypeGuid));

            lcsMessages.ColumnsSort = new[]
                                          {
                                              new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                                              new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc),
                                          };

            return lcsMessages;
        }

        /// <summary>
        /// Конструктор, создающий поток для отправки сообщений клиенту.
        /// </summary>
        /// <param name="waitScanDb4CallBackHandleFromParentThread"> Элемент, который разбудит создаваемый поток, если нужно будет закончить работу. </param>
        public SubscriptionThread(ManualResetEvent waitScanDb4CallBackHandleFromParentThread, IStatisticsService statistics, IDataService dataService, ILogger logger)
        {
            waitScanDb4CallBackHandle = waitScanDb4CallBackHandleFromParentThread;
            _statistics = statistics;
            _dataService = dataService;
            _logger = logger;
            ExecThread = new Thread(JobSendByCallback) { IsBackground = true };
            ldef = new ExternalLangDef();
        }

        /// <summary>
        /// Метод для отправки сообщений по подписке в отдельном потоке.
        /// </summary>
        /// <param name="subscriptionObject">
        /// Подписка, по которой будет производиться отправка сообщений.
        /// </param>
        public void JobSendByCallback(object subscriptionObject)
        {
            var subscription = (Subscription)subscriptionObject;
            ServiceHelper.TryWithExceptionLogging(
                () =>
                    {
                        ResetMessagesStatusBeforeStart(_dataService, _logger);
                        while (IsActive)
                        {
                            UpdateMessagesStatus(_dataService, _logger);

                            if (!IsActive)
                                continue;

                            List<Guid> messagesPks = GetMessagesToSend(_dataService, _statistics, MessagesPerQuery).ToList();
                            if (messagesPks.Count > 0)
                            {
                                foreach (var pk in messagesPks)
                                {
                                    if (!IsActive)
                                        break;

                                    var parameters = new SendingTaskParams
                                                         {
                                                             MessagePk = pk,
                                                             Subscription = subscription,
                                                             AdditionalTimeout = AdditionalTimeout
                                                         };
                                    const TaskCreationOptions Options = TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning;
                                    Task<bool> task = new Task<bool>(o => SendMessageWithFailHandling(o, _statistics, _dataService, _logger), parameters, Options);
                                    sendingTasks[pk] = task;
                                    task.Start();
                                }

                                continue;
                            }

                            if (sendingTasks.Count == 0)
                            {
                                IsActive = false;
                                continue;
                            }

                            // Если дошли до сюда, значит нет сообщений для отправки, но есть находящиеся в отправке или
                            // отложенные. В этом случае необходимо подождать ScanningTimeout и продолжить работу.
                            waitScanDb4CallBackHandle.WaitOne(ScanningTimeout);
                        }
                    },
                null,
                "Ошибка при выполнении потока отправки сообщений по callback",
                subscription.Client,
                null,
                _logger);
        }

        /// <summary>
        /// Метод для внешнего использования. Ожидает, пока текущие сообщения не будут отправлены, и завершает работу потока отправки.
        /// </summary>
        public void WaitForHandlingAndStop()
        {
            if (!IsExecuting)
                return;

            IsActive = false;

            // Ожидание пока поток отправки не завершит работу, разобравшись с текущими сообщениями.
            while (IsExecuting)
            {
                // ToDo: вынести в конфиг, либо подобрать существующий параметр продолжительности сна.
                if (waitScanDb4CallBackHandle.WaitOne(1000))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Запустить выполнение потока отправки сообщений по callback.
        /// </summary>
        public void StartExecution()
        {
            IsActive = true;
            ExecThread.Start(Subscription);
        }

        /// <summary>
        /// Метод для выполнения отправки сообщения в отдельном потоке. Свойство <see cref="Сообщение.Отправляется"/> должно быть
        /// установлено в <c>true</c> заранее.
        /// </summary>
        /// <param name="state">Параметры для задачи отправки сообщения.</param>
        /// <returns>Была ли выполнена отправка сообщения.</returns>
        private static bool SendMessageWithFailHandling(object state, IStatisticsService statisticsService, IDataService dataService, ILogger logger)
        {
            var parameters = (SendingTaskParams)state;

            Message message = null;
            Stopwatch stopwatch = null;
            long time;
            Action loadMessageDelegate = () =>
                {
                    message = new Message();
                    message.SetExistObjectPrimaryKey(parameters.MessagePk);

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    dataService.LoadObject(Message.Views.MessageEditView, message);

                    stopwatch.Stop();
                    time = stopwatch.ElapsedMilliseconds;
                    statisticsService.NotifyAvgTimeSql(null, (int)time, "SubscriptionThread.SendMessageWithFailHandling() load Messages.");
                };

            if (!ServiceHelper.TryWithExceptionLogging(loadMessageDelegate, null, "Ошибка при отправке сообщения по callback", parameters.Subscription.Client, null, logger))
                return false;

            IMessageSender messageSender = new MessageSenderCreator(logger, true).GetMessageSender(parameters.Subscription);

            statisticsService.NotifyIncConnectionCount(parameters.Subscription);
            stopwatch = new Stopwatch();
            stopwatch.Start();
            var send = messageSender.SendMessage(message);
            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSent(parameters.Subscription, (int)time);
            statisticsService.NotifyDecConnectionCount(parameters.Subscription);

            if (!send)
            {
                statisticsService.NotifyErrorOccurred(parameters.Subscription);
                return false;
            }

            logger.LogOutgoingMessage(message);
            statisticsService.NotifyMessageSent(parameters.Subscription);
            return true;
        }

        /// <summary>
        /// Сбросить статусы "Отправляется" сообщений текущей подписки перед началом цикла отправки сообщений.
        /// Статусы могут остаться с предыдущего раза, если поток подписки упал и не обновил их.
        /// </summary>
        /// <param name="ds">Сервис данных, использующийся для работы с сообщениями.</param>
        private void ResetMessagesStatusBeforeStart(IDataService ds, ILogger logger)
        {
            LoadingCustomizationStruct lcs = GetSubscriptionLcs(Message.Views.MessageLightView);
            lcs.LimitFunction = ldef.GetFunction(
                ldef.funcAND,
                lcs.LimitFunction,
                ldef.GetFunction(ldef.funcEQ, new VariableDef(ldef.BoolType, Information.ExtractPropertyPath<Message>(x => x.IsSending)), true));

            DataObject[] messages = ds.LoadObjects(lcs);
            foreach (Message m in messages)
                m.IsSending = false;

            ServiceHelper.UpdateObjects(ds, ref messages, logger, _statistics);
        }

        /// <summary>
        /// Обновить статусы сообщений, отправка которых завершена.
        /// </summary>
        /// <param name="ds">Сервис данных, через который будет производиться работа с сообщениям.</param>
        private void UpdateMessagesStatus(IDataService ds, ILogger logger)
        {
            Dictionary<Guid, Task<bool>> completedTasks = sendingTasks.Where(x => x.Value.IsCompleted).Take(MessagesPerQuery).
                ToDictionary(pair => pair.Key, pair => pair.Value);

            if (completedTasks.Count == 0)
                return;

            DataObject[] messagesToUpdate = completedTasks.Keys.Select(
                x =>
                    {
                        var msg = new Message();
                        msg.SetExistObjectPrimaryKey(x);
                        return msg;
                    }).ToArray();
            ds.LoadObjects(messagesToUpdate, Message.Views.MessageLightView, false);

            // Обновление статусов сообщений.
            foreach (Message msg in messagesToUpdate)
            {
                Task<bool> task = completedTasks[new Guid(msg.__PrimaryKey.ToString())];
                if (task.Status == TaskStatus.RanToCompletion && task.Result)
                {
                    msg.SetStatus(ObjectStatus.Deleted);
                }
                else
                {
                    msg.ErrorCount++;
                    msg.IsSending = false;

                    // Время следующей отправки прямо пропорционально числу неудачных попыток.
                    int timeoutInMinutes = AdditionalTimeout * msg.ErrorCount;
                    msg.SendingTime = DateTime.Now + new TimeSpan(0, timeoutInMinutes, 0);
                }
            }

            ServiceHelper.UpdateObjects(ds, ref messagesToUpdate, logger, _statistics);

            // Удаление информации о сообщениях, статусы которых были удачно обновлены.
            foreach (Guid pk in completedTasks.Keys)
            {
                Task<bool> temp;
                sendingTasks.TryRemove(pk, out temp);
            }
        }

        /// <summary>
        /// Период времени, через который последует очередная попытка отправить сообщение (в минутах).
        /// Закэшированное значение из конфига.
        /// </summary>
        private int? additionalTimeout;

        /// <summary>
        /// Период времени, через который последует очередная попытка отправить сообщение (в минутах).
        /// После единичного вычитывания из конфига кэшируется.
        /// </summary>
        private int AdditionalTimeout
        {
            get
            {
                if (additionalTimeout == null)
                {
                    string configString = ConfigurationManager.AppSettings["AdditionalTimeout"];
                    int result;
                    if (string.IsNullOrWhiteSpace(configString) || !Int32.TryParse(configString, out result))
                    {
                        _logger.LogInformation(null, "В конфиге не задано значение AdditionalTimeout. По умолчанию определено как 2.");
                        result = 2;
                    }

                    additionalTimeout = result;
                }

                return additionalTimeout.Value;
            }
        }

        /// <summary>
        /// Получить первичные ключи сообщений, соответствующих текущей подписке, которые нужно отправить, и пометить их
        /// как отправляемые. Ограничение на количество загружаемых сообщений составляет 100.
        /// </summary>
        /// <param name="dataService">Сервис данных, посредством которого загружаются сообщения.</param>
        /// <param name="count">Количество первичных ключей, которое нужно вернуть. Если не задано или равно 0, не учитывается.</param>
        /// <returns>Первичные ключи сообщений, которые нужно отправить.</returns>
        private IEnumerable<Guid> GetMessagesToSend(IDataService dataService, IStatisticsService statisticsService, int count = 0)
        {
            LoadingCustomizationStruct lcs = GetSubscriptionLcs(Message.Views.MessageLightView);
            lcs.LimitFunction = ldef.GetFunction(
                ldef.funcAND,
                lcs.LimitFunction,
                ldef.GetFunction(
                    ldef.funcEQ,
                    new VariableDef(ldef.BoolType, Information.ExtractPropertyName<Message>(x => x.IsSending)),
                    false),
                ldef.GetFunction(
                    ldef.funcLEQ,
                    new VariableDef(ldef.DateTimeType, Information.ExtractPropertyName<Message>(x => x.SendingTime)),
                    DateTime.Now));
            if (count > 0)
                lcs.ReturnTop = count;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messages = dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "SubscriptionThread.GetMessagesToSend() load Messages.");

            // Чтобы сообщения не загружались повторно, они помечаются как отправляемые.
            foreach (Message message in messages)
                message.IsSending = true;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.UpdateObjects(ref messages);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "SubscriptionThread.GetMessagesToSend() update Messages.");

            return messages.Select(x => new Guid(x.__PrimaryKey.ToString()));
        }

        /// <summary>
        /// Получить экземпляр <see cref="LoadingCustomizationStruct"/> для загрузки сообщений по идентификаторам клиента и типа сообщения.
        /// </summary>
        /// <param name="view">
        /// Представление, которое будет использовано в создаваемой LCS.
        /// Если не задано, используется <see cref="Сообщение.Views.SB_СообщениеE"/>.
        /// </param>
        /// <returns>
        /// Структура для загрузки подписок с помощью сервиса данных.
        /// </returns>
        private LoadingCustomizationStruct GetSubscriptionLcs(View view = null)
        {
            return GetSubscriptionLcs(
                ldef,
                ((KeyGuid)Subscription.Client.__PrimaryKey).Guid,
                ((KeyGuid)Subscription.MessageType.__PrimaryKey).Guid,
                view);
        }
    }
}
