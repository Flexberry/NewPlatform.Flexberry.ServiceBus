namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.KeyGen;
    using ICSSoft.STORMNET.Windows.Forms;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

    /// <summary>
    /// Статический класс для хранения и поддержания актуальности коллекции активных (срок действия которых не истек) на данный момент подписок.
    /// </summary>
    internal class CachedSubscriptionsManager : BaseServiceBusComponent, ISubscriptionsManager, ICacheable
    {
        private readonly ILogger _logger;

        private readonly IDataService _dataService;

        /// <summary>
        /// Язык для создания ограничений.
        /// </summary>
        private static readonly ExternalLangDef LangDef = ExternalLangDef.LanguageDef;

        /// <summary>
        /// Объект для разделения доступа к полю <see cref="Subscriptions"/>.
        /// </summary>
        private static readonly object SubscriptionsLockObject = new object();

        /// <summary>
        /// Список для хранения подписок.
        /// </summary>
        private static readonly List<Subscription> Subscriptions = new List<Subscription>();

        /// <summary>
        /// Таймер для синхронизации коллекции подписок с базой данных.
        /// </summary>
        private static Timer updateFromDbTimer;

        /// <summary>
        /// Период обновления информации о подписках из БД.
        /// </summary>
        public int UpdatePeriodMilliseconds { get; set; } = 60000;

        /// <summary>
        /// Statistics service
        /// </summary>
        private static IStatisticsService _statisticsService;

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="CachedSubscriptionsManager"/>.
        /// </summary>
        /// <param name="logger">Используемый компонент логирования.</param>
        /// <param name="dataService">Сервис данных.</param>
        public CachedSubscriptionsManager(ILogger logger, IDataService dataService, IStatisticsService statisticsService)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            _logger = logger;
            _dataService = dataService;
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Обновить данные о подписках и ограничениях с помощью запроса к БД.
        /// </summary>
        private void UpdateFromDb()
        {
            try
            {
                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Subscription), Subscription.Views.SendingByCallbackView);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                DataObject[] subscrs = _dataService.LoadObjects(lcs);

                stopwatch.Stop();
                long time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.UpdateFromDb() load Subscription.");

                lock (SubscriptionsLockObject)
                {
                    Subscriptions.Clear();
                    Subscriptions.AddRange(subscrs.Cast<Subscription>());
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Update subscriptions error", exception.ToString());
            }
        }

        /// <summary>
        /// Создать клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="name">Имя клиента.</param>
        /// <param name="address">Адрес, если ожидается, что клиент будет получать сообщения по callback.</param>
        public void CreateClient(string clientId, string name, string address = null)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));

            var client = new Client() { ID = clientId, Name = name, Address = address };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(client);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.CreateClient() update Client.");
        }

        /// <summary>
        /// Удалить клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно удалить.</param>
        public void DeleteClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));

            DataObject[] objectsToDelete;
            Stopwatch stopwatch;
            long time;
            lock (SubscriptionsLockObject)
            {
                var subscriptionsToDelete = Subscriptions.Where(x => x.Client.ID == clientId || CompareGuid2Str(((KeyGuid)x.Client.__PrimaryKey).Guid, clientId)).ToList();
                foreach (var subscr in subscriptionsToDelete)
                {
                    Subscriptions.Remove(subscr);
                    subscr.SetStatus(ObjectStatus.Deleted);
                }

                Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
                var client = new Client();
                client.SetExistObjectPrimaryKey(clientPk);
                var messageLcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageLightView);
                messageLcs.LimitFunction = LangDef.GetFunction(
                    LangDef.funcEQ,
                    new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)),
                    clientPk);

                stopwatch = new Stopwatch();
                stopwatch.Start();

                objectsToDelete = _dataService.LoadObjects(messageLcs);

                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.DeleteClient() load messages.");

                objectsToDelete = objectsToDelete.Concat(new DataObject[] { client }).ToArray();
                objectsToDelete.ForEach(obj => obj.SetStatus(ObjectStatus.Deleted));
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObjects(ref objectsToDelete);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.DeleteClient() update messages.");
        }

        /// <summary>
        /// Создать тип сообщений.
        /// </summary>
        /// <param name="msgTypeInfo">Информация о создаваемом типе сообщений: идентификатор, наименование, комментарий.</param>
        public void CreateMessageType(ServiceBusMessageType msgTypeInfo)
        {
            var msgType = new MessageType()
            {
                ID = msgTypeInfo.ID,
                Name = msgTypeInfo.Name,
                Description = msgTypeInfo.Description
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(msgType);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.CreateMessageType() update TypeMessage.");
        }

        /// <summary>
        /// Получить все существующие подписки.
        /// </summary>
        /// <param name="onlyActive">Вернуть только активные (дата прекращения больше текущей) подписки. По умолчанию <c>true</c>.</param>
        /// <returns>
        /// Найденные подписки.
        /// </returns>
        public IEnumerable<Subscription> GetSubscriptions(bool onlyActive = true)
        {
            lock (SubscriptionsLockObject)
            {
                return onlyActive ? Subscriptions.Where(x => x.ExpiryDate >= DateTime.Now).ToList() : Subscriptions.ToList();
            }
        }

        /// <summary>
        /// Получить все существующие подписки указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, для которого нужно получить подписки.</param>
        /// <param name="onlyActive">Вернуть только активные (дата прекращения больше текущей) подписки. По умолчанию <c>true</c>.</param>
        /// <returns>
        /// Найденные подписки указанного клиента.
        /// </returns>
        public IEnumerable<Subscription> GetSubscriptions(string clientId, bool onlyActive = true)
        {
            return GetSubscriptions(onlyActive).Where(x => x.Client.ID == clientId || CompareGuid2Str(((KeyGuid)x.Client.__PrimaryKey).Guid, clientId));
        }

        /// <summary>
        /// Получить активные callback-подписки (отправка по которым производится по инициативе шины).
        /// </summary>
        /// <param name="onlyActive">Возвращать только активные подписки.</param>
        /// <returns>
        /// Найденные callback-подписки.
        /// </returns>
        public IEnumerable<Subscription> GetCallbackSubscriptions(bool onlyActive = true)
        {
            return GetSubscriptions(onlyActive).Where(x => x.IsCallback);
        }

        /// <summary>
        /// Получить подписки для конкретного типа сообщений.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщений, для которого нужно получить подписи.</param>
        /// <param name="senderId">Идентификатор отправителя сообщений (его подписки не будут включены в результат). Если нужно получить все подписки, параметр указывать не нужно.</param>
        /// <returns>
        /// Найденные подписки для указанного типа сообщений.
        /// </returns>
        public IEnumerable<Subscription> GetSubscriptionsForMsgType(string messageTypeId, string senderId = null)
        {
            IEnumerable<Subscription> result = GetSubscriptions().Where(x => x.MessageType.ID == messageTypeId || CompareGuid2Str(((KeyGuid)x.MessageType.__PrimaryKey).Guid, messageTypeId));
            if (!string.IsNullOrEmpty(senderId))
                result = result.Where(x => x.Client.ID != senderId && !CompareGuid2Str(((KeyGuid)x.Client.__PrimaryKey).Guid, senderId));

            return result;
        }

        /// <summary>
        /// Подписать клиента на определенный тип сообщений или обновить подписку.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписку которого нужно обновить или создать.</param>
        /// <param name="messageTypeId">Тип сообщений подписки.</param><param name="isCallback">Является ли подписка callback.</param>
        /// <param name="transportType">Способ передачи сообщений, если подписка callback, иначе можно передать null.</param>
        /// <param name="expiryDate">Дата прекращения подписки. Если не указана, вычисляется как сумма текущей даты и параметра конфигурации UpdateForATime.</param>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно обновить или создать.</param>
        public void SubscribeOrUpdate(string clientId, string messageTypeId, bool isCallback, TransportType? transportType, DateTime? expiryDate = null, string subscriptionId = null)
        {
            Subscription[] subscriptions = GetSubscriptions(clientId, false).Where(x => x.MessageType.ID == messageTypeId || CompareGuid2Str(((KeyGuid)x.MessageType.__PrimaryKey).Guid, messageTypeId)).ToArray();
            Subscription subscription = subscriptionId == null ? subscriptions.FirstOrDefault() : subscriptions.FirstOrDefault(s => CompareGuid2Str(((KeyGuid)s.__PrimaryKey).Guid, subscriptionId));
            Stopwatch stopwatch;
            long time;
            if (subscription == null)
            {
                var langDef = ExternalLangDef.LanguageDef;

                LoadingCustomizationStruct clientLcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Client), Client.Views.EditView);
                clientLcs.LimitFunction = langDef.GetFunction(langDef.funcEQ, new VariableDef(langDef.StringType, Information.ExtractPropertyPath<Client>(x => x.ID)), clientId);

                stopwatch = new Stopwatch();
                stopwatch.Start();

                var client = _dataService.LoadObjects(clientLcs).Cast<Client>().FirstOrDefault();

                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.SubscribeOrUpdate() load Client.");

                if (client == null)
                    throw new ArgumentException("clientId");

                LoadingCustomizationStruct messageTypeLcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(MessageType), MessageType.Views.EditView);
                messageTypeLcs.LimitFunction = langDef.GetFunction(langDef.funcEQ, new VariableDef(langDef.StringType, Information.ExtractPropertyPath<MessageType>(x => x.ID)), messageTypeId);

                stopwatch = new Stopwatch();
                stopwatch.Start();

                var messageType = _dataService.LoadObjects(messageTypeLcs).Cast<MessageType>().FirstOrDefault();

                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.SubscribeOrUpdate() load TypeMessage.");

                if (messageType == null)
                    throw new ArgumentException("messageTypeId");

                subscription = new Subscription() { Client = client, MessageType = messageType };
                if (subscriptionId != null)
                {
                    subscription.__PrimaryKey = Guid.Parse(subscriptionId);
                }
            }

            subscription.IsCallback = isCallback;

            if (transportType != null)
            {
                subscription.TransportType = transportType.Value;
            }
            else if (isCallback)
            {
                throw new ArgumentNullException(nameof(transportType));
            }

            if (expiryDate != null)
            {
                subscription.ExpiryDate = expiryDate.Value;
            }
            else
            {
                ServiceHelper.UpdateStoppingDate(subscription);
            }

            if (subscription.GetStatus() == ObjectStatus.Created)
            {
                lock (SubscriptionsLockObject)
                {
                    Subscriptions.Add(subscription);
                }
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(subscription);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(subscription, (int)time, "CachedSubscriptionsManager.SubscribeOrUpdate() load Client.");
        }

        /// <summary>
        /// Обновить все подписки указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписки которого нужно обновить.</param>
        public void UpdateAllSubscriptions(string clientId)
        {
            IEnumerable<Subscription> subscriptions = GetSubscriptions(clientId, false).ToList();
            foreach (var s in subscriptions)
            {
                ServiceHelper.UpdateStoppingDate(s);
            }

            DataObject[] objectsToUpdate = subscriptions.Cast<DataObject>().ToArray();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObjects(ref objectsToUpdate);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "CachedSubscriptionsManager.UpdateAllSubscriptions() update Client.");
        }

        /// <summary>
        /// Обновить клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно обновить.</param>
        /// <param name="client">Новые данные клиента.</param>
        public void UpdateClient(string clientId, ServiceBusClient client)
        {
            Guid primaryKey = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
            Client currentClient = ServiceHelper.GetClient(primaryKey, _dataService, _statisticsService);

            if (client.Address != null)
            {
                currentClient.Address = client.Address;
            }

            if (client.Name != null)
            {
                currentClient.Name = client.Name;
            }

            if (client.Description != null)
            {
                currentClient.Description = client.Description;
            }

            if (client.DnsIdentity != null)
            {
                currentClient.DnsIdentity = client.DnsIdentity;
            }

            if (client.ConnectionsLimit != null)
            {
                currentClient.ConnectionsLimit = client.ConnectionsLimit;
            }

            if (client.SequentialSent != null)
            {
                bool sequentialSent = client.SequentialSent.Value;
                currentClient.SequentialSent = sequentialSent;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(currentClient);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.UpdateClient() update client.");
        }

        /// <summary>
        /// Обновить тип сообщения.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="messageType">Новые данные типа сообщения.</param>
        public void UpdateMessageType(string messageTypeId, ServiceBusMessageType messageType)
        {
            Guid primaryKey = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statisticsService);
            MessageType currentMessageType = ServiceHelper.GetMessageType(primaryKey, _dataService, _statisticsService);

            if (messageType.Name != null)
            {
                currentMessageType.Name = messageType.Name;
            }

            if (messageType.Description != null)
            {
                currentMessageType.Description = messageType.Description;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(currentMessageType);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.UpdateMessageType() update messageType.");
        }

        /// <summary>
        /// Удалить тип сообщения.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        public void DeleteMessageType(string messageTypeId)
        {
            Guid primaryKey = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statisticsService);
            MessageType currentMessageType = ServiceHelper.GetMessageType(primaryKey, _dataService, _statisticsService);

            currentMessageType.SetStatus(ObjectStatus.Deleted);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(currentMessageType);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.DeleteMessageType() update messageType.");
        }

        /// <summary>
        /// Обновить подписку.
        /// </summary>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно обновить.</param>
        /// <param name="subscription">Новые данные подписки.</param>
        public void UpdateSubscription(string subscriptionId, ServiceBusSubscription subscription)
        {
            Subscription currentSubscription = new Subscription { __PrimaryKey = subscriptionId };
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.LoadObject(currentSubscription);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.UpdateSubscription() load subscription.");

            if (subscription.ExpiryDate != null)
            {
                currentSubscription.ExpiryDate = subscription.ExpiryDate.Value;
            }

            if (subscription.Description != null)
            {
                currentSubscription.Description = subscription.Description;
            }

            if (subscription.Callback != null)
            {
                bool callback = subscription.Callback.Value;
                currentSubscription.IsCallback = callback;
            }

            TransportType transportType;
            EnumCaption.TryGetValueFor(subscription.SendBy, out transportType);
            if (transportType != default(TransportType))
            {
                currentSubscription.TransportType = transportType;
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(currentSubscription);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.UpdateSubscription() update subscription.");
        }

        /// <summary>
        /// Удалить подписку.
        /// </summary>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно удалить.</param>
        public void DeleteSubscription(string subscriptionId)
        {
            Subscription currentSubscription = new Subscription { __PrimaryKey = subscriptionId };
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.LoadObject(currentSubscription);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.DeleteSubscription() load subscription.");

            currentSubscription.SetStatus(ObjectStatus.Deleted);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(currentSubscription);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.DeleteSubscription() update subscription.");
        }

        /// <summary>
        /// Подготовка к запуску компонента. После выполнения этого метода компонент должен быть
        /// способен к выполнению его методов из других компонентов.
        /// </summary>
        public override void Prepare()
        {
            UpdateFromDb();
        }

        /// <summary>
        /// Запуск компонента. В этом методе должны инициализироваться различные внешние сервисы, если
        /// они есть, создаваться потоки обработки.
        /// </summary>
        public override void Start()
        {
             updateFromDbTimer = new Timer(x => UpdateFromDb(), null, UpdatePeriodMilliseconds, UpdatePeriodMilliseconds);
        }

        /// <summary>
        /// Остановка компонента. Сервисы и потоки останавливаются.
        /// </summary>
        public override void Stop()
        {
            updateFromDbTimer.Dispose();
        }

        /// <summary>
        /// Clear cached data.
        /// </summary>
        public void ClearCache()
        {
            Subscriptions.Clear();
        }

        private static bool CompareGuid2Str(Guid guid, string str)
        {
            Guid guid2;
            if (!Guid.TryParse(str, out guid2))
                return false;
            else
                return guid == guid2;
        }
    }
}
