namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;

    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы с шиной.
    /// </summary>
    internal static class ServiceHelper
    {
        /// <summary>
        /// Имя тега для передачи имени отправителя в сообщениях.
        /// </summary>
        private const string SenderNameTag = "senderName";

        /// <summary>
        /// Language Def для формирования ограничений.
        /// </summary>
        private static readonly ExternalLangDef LangDef = ExternalLangDef.LanguageDef;

        /// <summary>
        /// Получить целочисленное значение настройки из AppSettings в конфигурационном файле.
        /// </summary>
        /// <param name="paramName">Имя настройки, которую надо прочитать.</param>
        /// <param name="defaultValue">Значение по умолчанию. Будет использовано, если не удалось получить целочисленное значение из конфигурационного файла.</param>
        /// <returns>Итоговое значение параметра конфигурации.</returns>
        public static int GetIntConfigParam(string paramName, int defaultValue, ILogger logger)
        {
            string parameterValue = ConfigurationManager.AppSettings[paramName];
            int result;
            if (!int.TryParse(parameterValue, out result))
            {
                result = defaultValue;
                logger.LogError(
                    $"Не найден или некорректно задан параметр конфигурации '{paramName}'",
                    $"Будет использовано значение по умолчанию: {defaultValue}.");
            }

            return result;
        }

        /// <summary>
        /// Создать сообщение для отправки клиенту посредством WCF-сервиса по сообщению, хранящемуся в шину.
        /// </summary>
        /// <param name="formTime">Время формирования сообщения в шине.</param>
        /// <param name="messageTypeID">Идентификатор типа собщения.</param>
        /// <param name="msgBody">Тело сообещния.</param>
        /// <param name="senderName">Имя отправителя сообщения.</param>
        /// <param name="group">Имя группы сообщения.</param>
        /// <param name="tags">Теги собщения.</param>
        /// <param name="attachment">Вложение сообщения.</param>
        /// <returns>Сформированный объект для отправки сообщения.</returns>
        public static ServiceBusMessage CreateWcfMessageFromEsb(
            DateTime formTime,
            String messageTypeID,
            String msgBody,
            String senderName,
            String group,
            Dictionary<string, string> tags,
            byte[] attachment)
        {
            var msg = new ServiceBusMessage
            {
                                 MessageFormingTime = formTime,
                                 MessageTypeID = messageTypeID,
                                 Body = msgBody,
                                 Attachment = attachment,
                                 SenderName = senderName,
                                 Group = group,
                                 Tags = tags
                             };

            return msg;
        }

        /// <summary>
        /// Создать сообщение для отправки клиенту посредством HTTP по сообщению, хранящемуся в шину.
        /// </summary>
        /// <param name="id">Идентификатор сообщения, хранящегося в шине.</param>
        /// <param name="formTime">Время формирования сообщения в шине.</param>
        /// <param name="messageTypeID">Идентификатор типа собщения.</param>
        /// <param name="msgBody">Тело сообещния.</param>
        /// <param name="senderName">Имя отправителя сообщения.</param>
        /// <param name="groupID">Имя группы сообщения.</param>
        /// <param name="tag">Теги собщения.</param>
        /// <param name="attachment">Вложение сообщения.</param>
        /// <returns>Сформированный объект для отправки сообщения.</returns>
        public static HttpMessageFromEsb CreateHttpMessageFromEsb(
            string id,
            DateTime formTime,
            String messageTypeID,
            String msgBody,
            String senderName,
            String groupID,
            Dictionary<string, string> tag,
            byte[] attachment)
        {
            var msg = new HttpMessageFromEsb
            {
                Id = id,
                MessageFormingTime = formTime,
                MessageTypeID = messageTypeID,
                Body = msgBody,
                Attachment = attachment,
                SenderName = senderName,
                GroupID = groupID,
                Tags = tag
            };

            return msg;
        }

        /// <summary>
        /// Получение словаря тегов из строки. Теги разделяются знаком «;».
        /// Название тега и значение разделяются знаком «:».
        /// </summary>
        /// <param name="msg">
        /// Сообщение.
        /// </param>
        /// <returns>
        /// Словарь тэгов.
        /// </returns>
        public static Dictionary<string, string> GetTagDictionary(Message msg)
        {
            if (msg.Tags == null)
                return new Dictionary<string, string>();
            else
                return msg.Tags
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Split(new[] { ':' }, 2))
                    .ToDictionary(t => t.First(), t => t.Last());
        }

        /// <summary>
        /// Функция для преобразования переданного идентификатора клиента в первичный ключ.
        /// </summary>
        /// <param name="id">Идентификатор клиента (дружественный идентификатор либо первичный ключ).</param>
        /// <returns>Первичный ключ клиента, соответствующего идентификатору.</returns>
        public static Guid ConvertClientIdToPrimaryKey(string id, IDataService dataService, IStatisticsService statisticsService)
        {
            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Client), Information.GetView("ListView", typeof(Client)));
            lcs.LimitFunction = LangDef.GetFunction(LangDef.funcLike, new VariableDef(LangDef.StringType, Information.ExtractPropertyPath<Client>(x => x.ID)), id);
            lcs.ReturnTop = 1;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] clients = dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.ConvertClientIdToPrimaryKey() load Client.");

            if (clients.Length > 0)
                return Guid.Parse(clients[0].__PrimaryKey.ToString());

            Guid pk;
            if (Guid.TryParse(id, out pk))
                return pk;

            throw new InvalidOperationException(String.Format("Клиент с идентификатором \'{0}\' не найден.", id));
        }

        /// <summary>
        /// Функция для преобразования переданного идентификатора типа сообщения в первичный ключ.
        /// </summary>
        /// <param name="id">Идентификатор типа сообщения (дружественный идентификатор либо первичный ключ).</param>
        /// <returns>Первичный ключ типа сообщения, соответствующего идентификатору.</returns>
        public static Guid ConvertMessageTypeIdToPrimaryKey(string id, IDataService dataService, IStatisticsService statisticsService)
        {
            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(MessageType), Information.GetView("ListView", typeof(MessageType)));
            lcs.LimitFunction = LangDef.GetFunction(LangDef.funcLike, new VariableDef(LangDef.StringType, Information.ExtractPropertyPath<MessageType>(x => x.ID)), id);
            lcs.ReturnTop = 1;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messageTypes = dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.ConvertMessageTypeIdToPrimaryKey() load TypeMessage.");

            if (messageTypes.Length > 0)
                return Guid.Parse(messageTypes[0].__PrimaryKey.ToString());

            Guid pk;
            if (Guid.TryParse(id, out pk))
                return pk;

            throw new InvalidOperationException(String.Format("Тип сообщения с идентификатором \'{0}\' не найден.", id));
        }

        /// <summary>
        /// Выполнить действие, отловив возможное исключение и записав его в лог приложения.
        /// </summary>
        /// <param name="action">Действие, которое нужно выполнить.</param>
        /// <param name="finallyAction">Действие, выполняемое после основного независимо от его успешности.</param>
        /// <param name="logMessageTitle">Заголовок сообщения лога, которое будет создано в случае исключения.</param>
        /// <param name="lastClient">Последний клиент, с котороым велась работа (будет указан в сообщении лога).</param>
        /// <param name="lastMsg">Последнее сообщение, с которым велась работа (будет указано в сообщении лога).</param>
        /// <returns>Успешно ли было выполнено действие (если произошло исключение, вернется <c>false</c>, иначе <c>true</c>).</returns>
        public static bool TryWithExceptionLogging(
            Action action,
            Action finallyAction,
            string logMessageTitle,
            Client lastClient,
            Message lastMsg,
            ILogger logger)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.LogUnhandledException(e, lastMsg, logMessageTitle);
                return false;
            }
            finally
            {
                if (finallyAction != null)
                    finallyAction();
            }

            return true;
        }

        /// <summary>
        /// Функция для получения структуры для загрузки сообщений по указанному ограничению.
        /// </summary>
        /// <param name="limFunc">
        /// Ограничение на загружаемые сообщения.
        /// </param>
        /// <returns>
        /// Сформированная структура для загрузки сообщений.
        /// </returns>
        public static LoadingCustomizationStruct GetMessagesLcs(Function limFunc)
        {
            // ToDo: использовать статическое представление.
            var msgView = new View { DefineClassType = typeof(Message) };
            msgView.AddProperty(Information.ExtractPropertyPath<Message>(x => x.Recipient));
            msgView.AddProperty(Information.ExtractPropertyPath<Message>(x => x.MessageType));

            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), msgView);
            lcs.LimitFunction = limFunc;
            return lcs;
        }

        /// <summary>
        /// Установка поля "Отправитель" в создавамом сообщении. Берется из тега "senderName" или, если он не указан,
        /// то вычитывается клиент по идентификатору, указанному в отправляемом сообщении.
        /// </summary>
        /// <param name="messageFor">Сообщение, пришедшее в шину.</param>
        /// <param name="esb">Создаваемое в БД сообщение.</param>
        /// <param name="sender">Отправитель, если он уже известен.</param>
        public static void AddSenderToMessage(ServiceBusMessage messageFor, Message esb, Client sender, IDataService dataService, ILogger logger, IStatisticsService statisticsService)
        {
            var senderKnown = false;
            if (messageFor.Tags != null && messageFor.Tags.ContainsKey(SenderNameTag))
            {
                esb.Sender = messageFor.Tags[SenderNameTag];
                senderKnown = true;
            }

            if (senderKnown)
                return;

            if (sender == null)
            {
                sender = new Client { __PrimaryKey = ConvertClientIdToPrimaryKey(messageFor.ClientID, dataService, statisticsService) };
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    dataService.LoadObject(sender);

                    stopwatch.Stop();
                    long time = stopwatch.ElapsedMilliseconds;
                    statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.ConvertMessageTypeIdToPrimaryKey() load TypeMessage.");
                }
                catch (Exception ex)
                {
                    logger.LogUnhandledException(ex, esb, "Не указан ключ клиента", string.Format("Не указан ключ клиента {0} при передаче сообщения.\n", sender.Name));
                    return;
                }
            }

            esb.Sender = sender.Name;

            if (messageFor.Tags == null)
                messageFor.Tags = new Dictionary<string, string>();
            messageFor.Tags.Add(SenderNameTag, sender.Name);
        }

        /// <summary>
        /// Загрузить клиента по первичному ключу.
        /// </summary>
        /// <param name="clientPrimaryKey">Первичный ключ загружаемого клиента.</param>
        /// <returns>Найденный в БД клиент.</returns>
        public static Client GetClient(Guid clientPrimaryKey, IDataService dataService, IStatisticsService statisticsService)
        {
            var client = new Client { __PrimaryKey = clientPrimaryKey };

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                dataService.LoadObject(client);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.GetClient() load Client.");
            }
            catch
            {
                client = new Bus { __PrimaryKey = clientPrimaryKey };

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                dataService.LoadObject(client);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.GetClient() load SB.");
            }

            return client;
        }

        /// <summary>
        /// Загрузить тип сообщения по первичному ключу.
        /// </summary>
        /// <param name="messageTypePrimaryKey">Первичный ключ загружаемого типа сообщения.</param>
        /// <returns>Тип сообщения с заданным первичным ключом.</returns>
        public static MessageType GetMessageType(Guid messageTypePrimaryKey, IDataService dataService, IStatisticsService statisticsService)
        {
            var messageType = new MessageType { __PrimaryKey = messageTypePrimaryKey };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.LoadObject(messageType);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.GetMessageType() load TypeMessage.");

            return messageType;
        }

        /// <summary>
        /// Перенести теги из принятого сообщения в сохраняемое в БД сообщение.
        /// </summary>
        /// <param name="messageFor">Сообщение, пришедшее в шину.</param>
        /// <param name="msg">Сообщение, сохраняемое в БД.</param>
        public static void SaveTag(ServiceBusMessage messageFor, Message msg)
        {
            // Получение словаря тегов из строки.
            var tags = GetTagDictionary(msg);

            // Обновление тега «sendingWay».
            const string SendingWayTagName = "sendingWay";
            if (tags.ContainsKey(SendingWayTagName))
                tags[SendingWayTagName] += "/" + messageFor.ClientID;
            else
                tags[SendingWayTagName] = messageFor.ClientID;

            // Перенос тегов из переданных шине параметров в объект базы данных.
            if (messageFor.Tags != null)
            {
                foreach (var tagToUpdate in messageFor.Tags)
                    tags[tagToUpdate.Key] = tagToUpdate.Value;
            }

            // Сохранение тегов в соответствующем поле.
            msg.Tags = string.Join(";", tags.Select(t => string.Format("{0}:{1}", t.Key, t.Value)).ToArray());
        }

        /// <summary>
        /// Проинициализировать сообщение с группой для сохранения в БД.
        /// </summary>
        /// <param name="messageFor">Сообщение, пришедшее в шину.</param>
        /// <param name="subscription">Подписка, по которой осуществляется создание сообщения в БД.</param>
        /// <param name="messageWithGroup">Сообщение, сохраняемое в БД.</param>
        /// <param name="groupName">Имя группы сообщения.</param>
        public static void SetMessageWithGroupValues(ServiceBusMessage messageFor, Subscription subscription, Message messageWithGroup, string groupName, IDataService dataService, ILogger logger, IStatisticsService statisticsService)
        {
            messageWithGroup.Group = groupName;
            messageWithGroup.Body = messageFor.Body;
            messageWithGroup.Priority = messageFor.Priority;

            AddSenderToMessage(messageFor, messageWithGroup, null, dataService, logger, statisticsService);
            SaveTag(messageFor, messageWithGroup);

            messageWithGroup.BinaryAttachment = messageFor.Attachment;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.LoadObject(subscription.MessageType);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(subscription, (int)time, "ServiceHelper.SetMessageWithGroupValues() load TypeMessage.");

            messageWithGroup.MessageType = subscription.MessageType;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.LoadObject(subscription.Client);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(subscription, (int)time, "ServiceHelper.SetMessageWithGroupValues() load Client.");

            messageWithGroup.Recipient = subscription.Client;

            AddSenderToMessage(messageFor, messageWithGroup, null, dataService, logger, statisticsService);
            messageWithGroup.ReceivingTime = DateTime.Now;
        }

        /// <summary>
        /// Продлить подписку на заданный в конфигурации период времени.
        /// </summary>
        /// <param name="subscription">Подписка, дату прекращения которой нужно продлить.</param>
        public static void UpdateStoppingDate(Subscription subscription)
        {
            subscription.ExpiryDate = DateTime.Now + TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["UpdateForATime"]));
        }

        /// <summary>
        /// Обновление объекта.
        /// При этом, если произошла ошибка взаимоблокировки транзакций, то попытка обновить повторится.
        /// </summary>
        /// <param name="dataService">Сервис данных, через который происходит обновление БД.</param>
        /// <param name="dataObject">Объект данных, который требуется обновить.</param>
        public static void UpdateObject(IDataService dataService, DataObject dataObject, ILogger logger, IStatisticsService statisticsService)
        {
            var done = false;
            while (!done)
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    dataService.UpdateObject(dataObject);

                    stopwatch.Stop();
                    long time = stopwatch.ElapsedMilliseconds;
                    statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.UpdateObject() update DataObject.");
                }
                catch (Exception ex)
                {
                    if (ex is ExecutingQueryException)
                        ex = ex.InnerException;

                    // Ошибка взаимоблокировки транзакций.
                    if (ex is SqlException && ((SqlException)ex).Number == 1205)
                        continue;

                    logger.LogUnhandledException(ex, null, string.Format("При обновлении объекта {0} в БД возникло исключение.", dataObject == null ? "null" : dataObject.GetType().FullName));
                    continue;
                }

                done = true;
            }
        }

        /// <summary>
        /// Обновление множества объектов с обработкой ситуации взаимоблокировки транзакций и логированием
        /// прочих ошибок в системный лог.
        /// </summary>
        /// <param name="dataService">Сервис данных, через который происходит обновление БД.</param>
        /// <param name="dataObjects">Объекты данных, которые требуется обновить.</param>
        public static void UpdateObjects(IDataService dataService, ref DataObject[] dataObjects, ILogger logger, IStatisticsService statisticsService)
        {
            if (dataObjects == null)
                throw new ArgumentNullException("dataObjects");

            if (dataObjects.Length == 0)
                return;

            var done = false;
            while (!done)
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    dataService.UpdateObjects(ref dataObjects);

                    stopwatch.Stop();
                    long time = stopwatch.ElapsedMilliseconds;
                    statisticsService.NotifyAvgTimeSql(null, (int)time, "ServiceHelper.UpdateObjects() update DataObject.");
                }
                catch (Exception ex)
                {
                    if (ex is ExecutingQueryException)
                        ex = ex.InnerException;

                    // Ошибка взаимоблокировки транзакций.
                    if (ex is SqlException && ((SqlException)ex).Number == 1205)
                        continue;

                    logger.LogUnhandledException(ex, null, "При обновлении объектов в БД возникло исключение.");
                    continue;
                }

                done = true;
            }
        }
    }
}
