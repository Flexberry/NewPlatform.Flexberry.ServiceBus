namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Exceptions;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.FunctionalLanguage.SQLWhere;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;
    using Npgsql;
    using SortOrder = ICSSoft.STORMNET.Business.SortOrder;

    /// <summary>
    /// Base abstract implementation of <see cref="ISendingManager"/>.
    /// </summary>
    internal abstract class BaseSendingManager : BaseServiceBusComponent, ISendingManager
    {
        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly IStatisticsService _statistics;

        private readonly IDataService _dataService;

        private readonly ILogger _logger;

        protected readonly MessageSenderCreator MessageSenderCreator;


        /// <summary>
        /// Язык для создания ограничений.
        /// </summary>
        private static readonly ExternalLangDef _langDef = ExternalLangDef.LanguageDef;

        /// <summary>
        /// Fix incorrect message statuses on start.
        /// </summary>
        public bool ClearMessageStatusOnStart { get; set; } = false;

        /// <summary>
        /// Time between attempts to send delayed messages.
        /// </summary>
        public int ScanningPeriodMilliseconds { get; set; } = 10000;

        public BaseSendingManager(ISubscriptionsManager subscriptionsManager, IStatisticsService statistics, IDataService dataService, ILogger logger)
        {
            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (statistics == null)
                throw new ArgumentNullException(nameof(statistics));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            _subscriptionsManager = subscriptionsManager;
            _statistics = statistics;
            _dataService = dataService;
            _logger = logger;
            MessageSenderCreator = new MessageSenderCreator(_logger);
        }

        public abstract void QueueForSending(Message msg);

        /// <summary>
        /// Получить текущее количество неотправленных сообщений для указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Количество неотправленных сообщений.</returns>
        public virtual int GetCurrentMessageCount(string clientId)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Function limFunc = _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.GetObjectsCount(ServiceHelper.GetMessagesLcs(limFunc));

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetCurrentMessageCount(string clientId) load count messages.");

            return dobjs;
        }

        /// <summary>
        /// Получить текущее количество неотправленных сообщений указанного типа для указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Тип сообщений.</param>
        /// <returns>Количество неотправленных сообщений.</returns>
        public virtual int GetCurrentMessageCount(string clientId, string messageTypeId)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            var limFunc = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.GetObjectsCount(ServiceHelper.GetMessagesLcs(limFunc));

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetCurrentMessageCount(string clientId, string messageTypeId) load count messages.");

            return dobjs;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public virtual MessageInfoFromESB[] GetMessagesInfo(string clientId, int maxCount = 0)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageLightView);

            if (maxCount > 0)
                lcs.ReturnTop = maxCount;

            lcs.LimitFunction = _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk);

            lcs.ColumnsSort = new[]
            {
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc)
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messages = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetMessagesInfo(string clientId) load messages.");

            MessageInfoFromESB[] esbMessages = new MessageInfoFromESB[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                var msg = (Message)messages[i];

                esbMessages[i] = new MessageInfoFromESB
                {
                    Id = msg.__PrimaryKey.ToString(),
                    MessageTypeID = msg.MessageType.ID,
                    MessageFormingTime = msg.ReceivingTime,
                    Priority = msg.Priority
                };
            }

            return esbMessages;
        }

        /// <summary>
        /// Получить информацию о сообщениях указанного типа для указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщений.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public virtual MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, int maxCount = 0)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            // ToDo: использовать статическое представление.
            var view = new View { DefineClassType = typeof(Message) };
            view.AddProperty("MessageType");
            view.AddProperty("MessageType.ID");
            view.AddProperty("Recipient");
            view.AddProperty("ReceivingTime");
            view.AddProperty("Priority");

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), view);

            if (maxCount > 0)
                lcs.ReturnTop = maxCount;

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk));

            lcs.ColumnsSort = new[]
            {
                new ColumnsSortDef("Priority", SortOrder.Desc),
                new ColumnsSortDef("ReceivingTime", SortOrder.Asc)
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var messageObjects = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetMessagesInfo(string clientId, string messageTypeId) load messages.");

            return messageObjects.Cast<Message>().Select(x => new MessageInfoFromESB()
            {
                Id = x.__PrimaryKey.ToString(),
                MessageTypeID = x.MessageType.ID,
                MessageFormingTime = x.ReceivingTime,
                Priority = x.Priority
            }).ToArray();
        }

        /// <summary>
        /// Получить информацию о сообщениях.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="groupName">Имя группы сообщения.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public virtual MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, string groupName, int maxCount = 0)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            // ToDo: использовать статическое представление.
            var view = new View { DefineClassType = typeof(Message) };
            view.AddProperty("MessageType");
            view.AddProperty("MessageType.ID");
            view.AddProperty("Recipient");
            view.AddProperty("ReceivingTime");
            view.AddProperty("Priority");
            view.AddProperty("Group");

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), view);

            if (maxCount > 0)
                lcs.ReturnTop = maxCount;

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.StringType, Information.ExtractPropertyPath<Message>(x => x.Group)), groupName));

            lcs.ColumnsSort = new[]
            {
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc)
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var messageObjects = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetMessagesInfo(string clientId, string messageTypeId, string groupName) load messages.");

            return messageObjects.Cast<Message>().Select(x => new MessageInfoFromESB()
            {
                Id = x.__PrimaryKey.ToString(),
                MessageTypeID = x.MessageType.ID,
                MessageFormingTime = x.ReceivingTime,
                Priority = x.Priority
            }).ToArray();
        }

        /// <summary>
        /// Получить информацию о сообщении.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="tags">Теги, которые должно содержать сообщение.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public virtual MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, string[] tags, int maxCount = 0)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            // ToDo: использовать статическое представление.
            var view = new View { DefineClassType = typeof(Message) };
            view.AddProperty("MessageType");
            view.AddProperty("MessageType.ID");
            view.AddProperty("Recipient");
            view.AddProperty("ReceivingTime");
            view.AddProperty("Priority");
            view.AddProperty("Tags");

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), view);

            if (maxCount > 0)
                lcs.ReturnTop = maxCount;

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk));

            lcs.ColumnsSort = new[]
            {
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc)
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var messageObjects = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.GetMessagesInfo(string clientId, string messageTypeId, string[] tags) load messages.");

            return messageObjects.Cast<Message>()
                .Where(
                m =>
                {
                    // Получение словаря тегов из строки.
                    var messageTags = ServiceHelper.GetTagDictionary(m);
                    return tags.All(messageTags.ContainsKey);
                })
                .Select(
                    x =>
                    new MessageInfoFromESB()
                    {
                        Id = x.__PrimaryKey.ToString(),
                        MessageTypeID = x.MessageType.ID,
                        MessageFormingTime = x.ReceivingTime,
                        Priority = x.Priority
                    })
                .ToArray();
        }

        /// <summary>
        /// Получить сообщение по его идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>Сообщение, или <c>null</c>, если не найдено сообщение, соответствующее идентификатору.</returns>
        public virtual Message ReadMessage(string id)
        {
            var msg = new Message();
            msg.SetExistObjectPrimaryKey(new KeyGuid(id));

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _dataService.LoadObject(Message.Views.MessageEditView, msg);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage() load message.");

            }
            catch (CantFindDataObjectException)
            {
                return null;
            }

            _logger.LogOutgoingMessage(msg);
            _statistics.NotifyMessageSent(_subscriptionsManager.GetSubscriptions(msg.Recipient.ID).Single(s => s.MessageType.ID == msg.MessageType.ID));

            return msg;
        }

        /// <summary>
        /// Получить сообщение по идентификатору получателя и типу сообщения, первое в очереди отправки.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <returns>Сообщение, или <c>null</c>, если указанным параметрам не соответствует ни одно сообщение.</returns>
        public virtual Message ReadMessage(string clientId, string messageTypeId)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView);

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk));

            lcs.ColumnsSort = new[]
            {
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc)
            };

            lcs.ReturnTop = 1;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messages = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage(string clientId, string messageTypeId) load messages.");

            if (messages.Length != 0)
            {
                var msg = (Message)messages[0];
                _logger.LogOutgoingMessage(msg);
                _statistics.NotifyMessageSent(_subscriptionsManager.GetSubscriptions(clientId).Single(s => s.MessageType.ID == messageTypeId));
                return msg;
            }

            return null;
        }

        /// <summary>
        /// Получить сообщение указанного типа для указанного получателя, соответствующее указанному индексу.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя сообщения.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="index">Индекс сообщения в отсортированном списке сообщений по приоритету и времени формирования.</param>
        /// <returns>Сообщение, либо <c>null</c>, если сообщение не найдено для заданных аргументов.</returns>
        public virtual Message ReadMessage(string clientId, string messageTypeId, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index cannot be less than zero.");

            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView);

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk));

            lcs.ColumnsSort = new[]
                {
                    new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.Priority), SortOrder.Desc),
                    new ColumnsSortDef(Information.ExtractPropertyPath<Message>(x => x.ReceivingTime), SortOrder.Asc)
                };

            int rowNumber = index + 1;
            lcs.RowNumber = new RowNumberDef(rowNumber, rowNumber);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messages = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage(string clientId, string messageTypeId, int index) load messages.");

            switch (messages.Length)
            {
                case 1:
                    var msg = (Message)messages[0];
                    _logger.LogOutgoingMessage(msg);
                    _statistics.NotifyMessageSent(_subscriptionsManager.GetSubscriptions(clientId).Single(s => s.MessageType.ID == messageTypeId));
                    return msg;

                case 0:
                    return null;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Получить сообщение указанного типа для указанного получателя, соответствующее заданному имени группы.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя сообщения.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="groupName">Имя группы сообщения.</param>
        /// <returns>Сообщение, либо <c>null</c>, если сообщение не найдено для заданных аргументов.</returns>
        public virtual Message ReadMessage(string clientId, string messageTypeId, string groupName)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView);

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.StringType, Information.ExtractPropertyPath<Message>(x => x.Group)), groupName),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] messages = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage(string clientId, string messageTypeId, string groupName) load messages.");

            if (messages.Length != 0)
            {
                var msg = (Message)messages[0];
                _logger.LogOutgoingMessage(msg);
                _statistics.NotifyMessageSent(_subscriptionsManager.GetSubscriptions(clientId).Single(s => s.MessageType.ID == messageTypeId));
                return msg;
            }

            return null;
        }

        /// <summary>
        /// Получить следующее сообщение с указанными тегами указанного типа для указанного получателя.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя сообщения.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="tags">Теги, присутствующие в сообщении.</param>
        /// <returns>Сообщение, если оно есть, или <c>null</c>.</returns>
        public virtual Message ReadMessage(string clientId, string messageTypeId, string[] tags)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statistics);
            IEnumerable<Message> messages;

            try
            {
                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView);

                lcs.LimitFunction = _langDef.GetFunction(
                    _langDef.funcAND,
                    _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), messageTypePk),
                    _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                DataObject[] dobjs = _dataService.LoadObjects(lcs);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage(string clientId, string messageTypeId, string[] tags) load messages.");

                messages = dobjs.Cast<Message>().Where(m =>
                {
                    // Получение словаря тегов из строки.
                    var messageTags = ServiceHelper.GetTagDictionary(m);
                    return tags.All(messageTags.ContainsKey);
                });
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e);
                throw;
            }

            var msg = messages.FirstOrDefault();
            if (msg != null)
                _statistics.NotifyMessageSent(_subscriptionsManager.GetSubscriptions(clientId).Single(s => s.MessageType.ID == messageTypeId));

            return msg;
        }

        /// <summary>
        /// Проверить существование уведомления о событии.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, для которого проверяется существование уведомления.</param>
        /// <param name="eventTypeId">Идентификатор типа события.</param>
        /// <returns><c>true</c>, если уведомление существует в БД, иначе <c>false</c>.</returns>
        public virtual bool CheckEventIsRaised(string clientId, string eventTypeId)
        {
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statistics);
            Guid eventTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(eventTypeId, _dataService, _statistics);

            var eventsView = new View { DefineClassType = typeof(Message) };
            eventsView.AddProperty(Information.ExtractPropertyPath<Message>(x => x.MessageType));
            eventsView.AddProperty(Information.ExtractPropertyPath<Message>(x => x.Recipient));

            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), eventsView);

            lcs.LimitFunction = _langDef.GetFunction(
                _langDef.funcAND,
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.MessageType)), eventTypePk),
                _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)), clientPk));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] events = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.ReadMessage(string clientId, string messageTypeId, string[] tags) load messages.");
            
            return events.Length != 0;
        }

        /// <summary>
        /// Удалить заданное сообщение из очереди.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения, которое нужно удалить.</param>
        /// <returns><c>true</c>, если сообщение было удалено, иначе <c>false</c>.</returns>
        public virtual bool DeleteMessage(string messageId)
        {
            try
            {
                Guid pk;
                if (!Guid.TryParse(messageId, out pk))
                    return false;

                var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageListView);
                lcs.LimitFunction = _langDef.GetFunction(_langDef.funcEQ, new VariableDef(_langDef.GuidType, SQLWhereLanguageDef.StormMainObjectKey), pk);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var count = _dataService.GetObjectsCount(lcs);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.DeleteMessage() load count messages.");

                if (count > 0)
                {
                    var message = new Message();
                    message.SetExistObjectPrimaryKey(pk);
                    message.SetStatus(ObjectStatus.Deleted);

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    _dataService.UpdateObject(message);

                    stopwatch.Stop();
                    time = stopwatch.ElapsedMilliseconds;
                    _statistics.NotifyAvgTimeSql(null, (int)time, "BaseSendingManager.DeleteMessage(string messageTypeId) update messages.");
                    
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex, null, "Не удалось удалить сообщение", "Не удалось удалить сообщение.\n");
                throw;
            }
        }

        public sealed override void Prepare()
        {
            if (ClearMessageStatusOnStart)
            {
                try
                {
                    CorrectMessagesStatus(_dataService, _logger);
                }
                catch (Exception ex)
                {
                    _logger.LogUnhandledException(ex);
                    throw;
                }
            }

            base.Prepare();
        }

        /// <summary>
        /// Массовая правка статусов сообщений (например, какие-то сообщения ошибочно остались отправляемыми).
        /// </summary>
        /// <param name="ds">Сервис данных, через которых можно обновлять данные.</param>
        /// <param name="logger">Логгер для выполнения операции.</param>
        private static void CorrectMessagesStatus(IDataService ds, ILogger logger)
        {
            // TODO: придумать, как быстро и массово обновить статусы сообщений без sql.
            if (ds is PostgresDataService)
            {
                var conn = new NpgsqlConnection(ds.CustomizationString);
                try
                {
                    const string SqlString = "UPDATE Сообщение SET Отправляется = false";
                    var sqlCommand = new NpgsqlCommand(SqlString, conn);
                    conn.Open();
                    sqlCommand.ExecuteNonQuery();

                    logger.LogInformation(null, "Успешно обновлены некорректные статусы сообщений в БД.");
                }
                catch (Exception ex)
                {
                    logger.LogUnhandledException(ex, null, "Ошибка при массовом обновлении статусов сообщений");
                }
                finally
                {
                    conn.Close();
                }

                return;
            }

            if (ds is MSSQLDataService)
            {
                var conn = new SqlConnection(ds.CustomizationString);
                try
                {
                    const string SqlString = "UPDATE [Сообщение] SET [Отправляется] = 0";
                    var sqlCommand = new SqlCommand(SqlString, conn);
                    conn.Open();
                    sqlCommand.ExecuteNonQuery();

                    logger.LogInformation(null, "Успешно обновлены некорректные статусы сообщений в БД.");
                }
                catch (Exception ex)
                {
                    logger.LogUnhandledException(ex, null, "Ошибка при массовом обновлении статусов сообщений");
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}