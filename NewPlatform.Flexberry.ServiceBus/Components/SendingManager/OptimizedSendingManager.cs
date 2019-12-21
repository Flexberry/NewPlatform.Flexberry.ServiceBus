namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    /// <summary>
    /// Класс для отправки сообщений посредством сервиса данных с уменьшенным количеством запросов к БД.
    /// </summary>
    internal class OptimizedSendingManager : BaseSendingManager
    {
        /// <summary>
        /// Язык для создания ограничений.
        /// </summary>
        private static readonly ExternalLangDef _langDef = ExternalLangDef.LanguageDef;

        private readonly ILogger _logger;

        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly IStatisticsService _statisticsService;

        private readonly IDataService _dataService;

        /// <summary>
        /// Таймер, выполняющий операцию сканирования БД на наличие сообщений, которые нужно отправить.
        /// </summary>
        private Timer _scanningTimer;

        /// <summary>
        /// Количество выполняющихся в данный момент задач отправки.
        /// </summary>
        private int _sendingTasksCount;

        /// <summary>
        /// Gets or sets a value indicating whether enable online state.
        /// </summary>
        public bool EnableOnlineState { get; set; } = false;

        /// <summary>
        /// The maximum number of tasks for sending messages.
        /// </summary>
        public int MaxTasks { get; set; } = 1000;

        /// <summary>
        /// Number of minutes to be added to delay before the next attempt to send message.
        /// </summary>
        public int AdditionalMinutesBetweenRetries { get; set; } = 3;

        public OptimizedSendingManager(ISubscriptionsManager subscriptionsManager, IStatisticsService statisticsService, IDataService dataService, ILogger logger)
            : base(subscriptionsManager, statisticsService, dataService, logger)
        {
            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            _logger = logger;
            _subscriptionsManager = subscriptionsManager;
            _statisticsService = statisticsService;
            _dataService = dataService;
        }

        /// <summary>
        /// Структура, передающаяся методу отправки сообщения при его вызове.
        /// </summary>
        private struct SendingTaskParam
        {
            /// <summary>
            /// Подписка, по которой отправляется сообщение.
            /// </summary>
            public Subscription Subscription;

            /// <summary>
            /// Сообщение, которое нужно отправить.
            /// </summary>
            public Message Message;
        }

        public override void Start()
        {
            base.Start();
            _scanningTimer = new Timer(ScanMessages, null, 0, ScanningPeriodMilliseconds);
        }

        /// <summary>
        /// Остановка компонента. Сервисы и потоки останавливаются.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            _scanningTimer.Dispose();
        }

        /// <summary>
        /// Добавить сообщение в отправку.
        /// </summary>
        /// <param name="msg">Сообщение, которое нужно отправить.</param>
        public override void QueueForSending(Message msg)
        {
            TryEnqueue(msg);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanningTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Попытаться добавить в очередь для отправки сообщение.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно отправить.</param>
        /// <returns>Было ли сообщение добавлено в очередь.</returns>
        private bool TryEnqueue(Message message)
        {
            Subscription subscription = _subscriptionsManager
                .GetSubscriptions(message.Recipient.ID)
                .FirstOrDefault(x => x.MessageType.ID == message.MessageType.ID);

            if (subscription != null && _sendingTasksCount < MaxTasks)
            {
                message.IsSending = true;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _dataService.UpdateObject(message);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(subscription, (int)time, "OptimizedSendingManager.TryEnqueue() update message.");

                Interlocked.Increment(ref _sendingTasksCount);
                if (EnableOnlineState)
                {
                    _statisticsService.NotifyIncConnectionCount(subscription, message);
                }
                else
                {
                    _statisticsService.NotifyIncConnectionCount(subscription);
                }

                Task<bool>.Factory.StartNew(SendMessage, new SendingTaskParam { Message = message, Subscription = subscription }, TaskCreationOptions.PreferFairness)
                    .ContinueWith(SendingTaskContinuation, message);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Метод отправки сообщения.
        /// </summary>
        /// <param name="paramObject">Объект, содержащий параметры отправки сообщений.</param>
        /// <returns>Успешно ли было отправлено сообщение.</returns>
        private bool SendMessage(object paramObject)
        {
            var param = (SendingTaskParam)paramObject;

            IMessageSender messageSender = MessageSenderCreator.GetMessageSender(param.Subscription);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var send = messageSender.SendMessage(param.Message);
            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSent(param.Subscription, (int)time);

            if (!send)
            {
                _statisticsService.NotifyErrorOccurred(param.Subscription);
                return false;
            }

            _logger.LogOutgoingMessage(param.Message);
            _statisticsService.NotifyMessageSent(param.Subscription);

            return true;
        }


        /// <summary>
        /// Метод отправки сообщений из БД, выполняющийся по таймеру.
        /// </summary>
        /// <param name="state">Состояние, передаваемое при вызове метода таймером.</param>
        private void ScanMessages(object state)
        {
            try
            {
                if (_sendingTasksCount >= MaxTasks)
                    return;

                IEnumerable<Subscription> subscriptions = _subscriptionsManager.GetCallbackSubscriptions().ToList();
                if (!subscriptions.Any())
                    return;

                // Условие, ограничивающее получателя и тип сообщения загружаемых сообщений в соответствии с имеющимися подписками.
                Function[] conditionsList = subscriptions
                    .Select(
                        s =>
                        _langDef.GetFunction(
                            _langDef.funcAND,
                            _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), s.Client.__PrimaryKey),
                            _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), s.MessageType.__PrimaryKey)))
                    .ToArray();
                Function subscriptionsCondition = _langDef.GetFunction(_langDef.funcOR, conditionsList);

                Function messagesLf = _langDef.GetFunction(
                    _langDef.funcAND,
                    subscriptionsCondition,
                    _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.BoolType, Information.ExtractPropertyPath<Message>(x => x.IsSending)), false),
                    _langDef.GetFunction(_langDef.funcLEQ, new VariableDef(_langDef.DateTimeType, Information.ExtractPropertyPath<Message>(x => x.SendingTime)), DateTime.Now));

                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView);
                lcs.LimitFunction = messagesLf;
                lcs.ReturnTop = MaxTasks - _sendingTasksCount;
                lcs.ColumnsSort = new[]
                {
                    new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Asc),
                    new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.SendingTime), SortOrder.Asc)
                };

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                DataObject[] messages = _dataService.LoadObjects(lcs);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "OptimizedSendingManager.ScanMessages() load messages.");
                
                int index = 0;
                while (index < messages.Length && TryEnqueue((Message)messages[index]))
                    index++;
            }
            catch (Exception exception)
            {
                _logger.LogError("Send message error", exception.ToString());
            }
        }

        /// <summary>
        /// Метод, выполняющийся после завершения задачи отправки сообщения.
        /// </summary>
        /// <param name="task">Задача, по завершению которой запущен метод.</param>
        /// <param name="messageObject">Сообщение, которое отправлялось в указанной задаче.</param>
        private void SendingTaskContinuation(Task<bool> task, object messageObject)
        {
            Interlocked.Decrement(ref _sendingTasksCount);

            var message = (Message)messageObject;
            Subscription subscription = _subscriptionsManager
                .GetSubscriptions(message.Recipient.ID)
                .FirstOrDefault(x => x.MessageType.ID == message.MessageType.ID);
            if (EnableOnlineState)
            {
                _statisticsService.NotifyDecConnectionCount(subscription, message);
            }
            else
            {
                _statisticsService.NotifyDecConnectionCount(subscription);
            }

            var existNewMessageWithGroup = false;
            if (!string.IsNullOrEmpty(message.Group))
            {
                LoadingCustomizationStruct lcs = MessageBS.GetMessagesWithGroupLCS(message.Recipient, message.MessageType, message.Group);
                lcs.LimitFunction = _langDef.GetFunction(
                    _langDef.funcAND,
                    _langDef.GetFunction(
                        _langDef.funcNEQ,
                        new VariableDef(_langDef.GuidType, ExternalLangDef.StormMainObjectKey),
                        ((KeyGuid)message.__PrimaryKey).Guid),
                    lcs.LimitFunction);

                existNewMessageWithGroup = _dataService.GetObjectsCount(lcs) > 0;
            }

            if (existNewMessageWithGroup || (task.Status == TaskStatus.RanToCompletion && task.Result))
            {
                message.SetStatus(ObjectStatus.Deleted);
            }
            else
            {
                message.ErrorCount++;
                message.IsSending = false;

                // Время следующей отправки прямо пропорционально числу неудачных попыток.
                int timeoutInMinutes = AdditionalMinutesBetweenRetries * message.ErrorCount;
                message.SendingTime = DateTime.Now + new TimeSpan(0, timeoutInMinutes, 0);
            }

            ServiceHelper.UpdateObject(_dataService, message, _logger, _statisticsService);
        }
    }
}
