namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

    /// <summary>
    /// Класс по умолчанию для управления подписками.
    /// </summary>
    internal class DefaultSubscriptionsManager : BaseServiceBusComponent, ISubscriptionsManager
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// Язык для создания ограничений.
        /// </summary>
        private static readonly ExternalLangDef LangDef = ExternalLangDef.LanguageDef;

        /// <summary>
        /// Statistics service
        /// </summary>
        private static IStatisticsService _statisticsService;

        public DefaultSubscriptionsManager(IDataService dataService, IStatisticsService statisticsService)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            _dataService = dataService;
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Возвращает базовую LCS для загрузки подписок.
        /// </summary>
        /// <returns><see cref="LoadingCustomizationStruct"/> с проставленным представлением и типом объектов..</returns>
        private static LoadingCustomizationStruct GetInitialLcs()
        {
            return LoadingCustomizationStruct.GetSimpleStruct(typeof(Subscription), Subscription.Views.SendingByCallbackView);
        }

        /// <summary>
        /// Создать клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="name">Имя клиента.</param>
        /// <param name="address">Адрес, если ожидается, что клиент будет получать сообщения по callback.</param>
        public void CreateClient(string clientId, string name, string address = null)
        {
            var client = new Client() { ID = clientId, Name = name, Address = address };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(client);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.CreateClient() update Client.");
        }

        /// <summary>
        /// Удалить клиента, его подписки и сообщения.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно удалить.</param>
        public void DeleteClient(string clientId)
        {
            DataObject[] objectsToDelete;
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
            var client = new Client();
            client.SetExistObjectPrimaryKey(clientPk);
            var messageLcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageLightView);
            messageLcs.LimitFunction = LangDef.GetFunction(
                LangDef.funcEQ,
                new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Message>(x => x.Recipient)),
                clientPk);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            objectsToDelete = _dataService.LoadObjects(messageLcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.DeleteClient() load Client.");

            objectsToDelete = objectsToDelete.Concat(new DataObject[] { client }).ToArray();
            objectsToDelete.ForEach(obj => obj.SetStatus(ObjectStatus.Deleted));

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObjects(ref objectsToDelete);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.DeleteClient() update Client.");
        }

        /// <summary>
        /// Создать тип сообщений.
        /// </summary>
        /// <param name="msgTypeInfo">Информация о создаваемом типе сообщений: идентификатор, наименование, комментарий.</param>
        public void CreateMessageType(ServiceBusMessageType msgTypeInfo)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(new MessageType { ID = msgTypeInfo.ID, Name = msgTypeInfo.Name, Description = msgTypeInfo.Description });

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.CreateMessageType() update TypeMessage.");
        }

        /// <summary>
        /// Получить все имеющиеся подписки.
        /// </summary>
        /// <param name="onlyActive">Возвращать ли только активные подписки (дата прекращения больше текущей).</param>
        /// <returns>Коллекция найденных подписок.</returns>
        public IEnumerable<Subscription> GetSubscriptions(bool onlyActive = true)
        {
            LoadingCustomizationStruct lcs = GetInitialLcs();
            if (onlyActive)
            {
                lcs.LimitFunction = LangDef.GetFunction(
                    LangDef.funcGEQ,
                    new VariableDef(LangDef.DateTimeType, Information.ExtractPropertyPath<Subscription>(x => x.ExpiryDate)),
                    DateTime.Now);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(lcs).Cast<Subscription>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.GetSubscriptions() load Subscription.");

            return dobjs;
        }

        /// <summary>
        /// Получить все существующие подписки указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, для которого нужно получить подписки.</param>
        /// <param name="onlyActive">Вернуть только активные (дата прекращения больше текущей) подписки. По умолчанию <c>true</c>.</param>
        /// <returns>Найденный подписки указанного клиента.</returns>
        public IEnumerable<Subscription> GetSubscriptions(string clientId, bool onlyActive = true)
        {
            LoadingCustomizationStruct lcs = GetInitialLcs();
            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
            Function limitFunction = LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Subscription>(x => x.Client)), clientPk);
            if (onlyActive)
            {
                lcs.LimitFunction = LangDef.GetFunction(
                    LangDef.funcAND,
                    limitFunction,
                    LangDef.GetFunction(LangDef.funcGEQ, new VariableDef(LangDef.DateTimeType, Information.ExtractPropertyPath<Subscription>(x => x.ExpiryDate)), DateTime.Now));
            }

            lcs.LimitFunction = limitFunction;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(lcs).Cast<Subscription>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.GetSubscriptions(string clientId) load Subscription.");

            return dobjs;
        }

        /// <summary>
        /// Получить все callback-подписки.
        /// </summary>
        /// <param name="onlyActive">Возвращать только активные подписки.</param>
        /// <returns>Коллекция найденных подписок.</returns>
        public IEnumerable<Subscription> GetCallbackSubscriptions(bool onlyActive = true)
        {
            LoadingCustomizationStruct lcs = GetInitialLcs();
            Function limitFunction = LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.BoolType, Information.ExtractPropertyPath<Subscription>(x => x.IsCallback)));
            if (onlyActive)
            {
                lcs.LimitFunction = LangDef.GetFunction(
                    LangDef.funcAND,
                    limitFunction,
                    LangDef.GetFunction(
                        LangDef.funcGEQ,
                        new VariableDef(LangDef.DateTimeType, Information.ExtractPropertyPath<Subscription>(x => x.ExpiryDate)),
                        DateTime.Now));
            }
            else
            {
                lcs.LimitFunction = limitFunction;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(lcs).Cast<Subscription>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.GetCallbackSubscriptions() load Subscription.");

            return dobjs;
        }

        /// <summary>
        /// Получить подписки с конкретным типом сообщений.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщений.</param>
        /// <param name="senderId">Идентификатор отправителя (его подписки не будут включены в результат).</param>
        /// <returns>Коллекция найденных подписок.</returns>
        public IEnumerable<Subscription> GetSubscriptionsForMsgType(string messageTypeId, string senderId = null)
        {
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statisticsService);

            LoadingCustomizationStruct lcs = GetInitialLcs();
            Function limitFunction = LangDef.GetFunction(
                LangDef.funcAND,
                LangDef.GetFunction(LangDef.funcGEQ, new VariableDef(LangDef.DateTimeType, Information.ExtractPropertyPath<Subscription>(x => x.ExpiryDate)), DateTime.Now),
                LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Subscription>(x => x.MessageType)), messageTypePk));
            if (!string.IsNullOrEmpty(senderId))
            {
                Guid senderPk = ServiceHelper.ConvertClientIdToPrimaryKey(senderId, _dataService, _statisticsService);
                lcs.LimitFunction = LangDef.GetFunction(
                    LangDef.funcAND,
                    limitFunction,
                    LangDef.GetFunction(
                        LangDef.funcNEQ,
                        new VariableDef(LangDef.StringType, Information.ExtractPropertyPath<Subscription>(x => x.Client)),
                        senderPk));
            }
            else
            {
                lcs.LimitFunction = limitFunction;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(lcs).Cast<Subscription>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.GetCallbackSubscriptions(string messageTypeId) load Subscription.");

            return dobjs;
        }

        /// <summary>
        /// Подписать клиента на определенный тип сообщений или обновить подписку.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписку которого нужно обновить или создать.</param>
        /// <param name="messageTypeId">Тип сообщений подписки.</param>
        /// <param name="isCallback">Является ли подписка callback.</param>
        /// <param name="transportType">Способ передачи сообщений, если подписка callback, иначе можно передать null.</param>
        /// <param name="expiryDate">Дата прекращения подписки. Если не указана, вычисляется как сумма текущей даты и параметра конфигурации UpdateForATime.</param>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно обновить или создать.</param>
        public void SubscribeOrUpdate(string clientId, string messageTypeId, bool isCallback, TransportType? transportType, DateTime? expiryDate = null, string subscriptionId = null)
        {
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Subscription), Subscription.Views.SubscriptionsManagerView);

            Guid clientPk = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
            Guid messageTypePk = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, _dataService, _statisticsService);

            lcs.LimitFunction = LangDef.GetFunction(
                LangDef.funcAND,
                LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Subscription>(x => x.Client)), clientPk),
                LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.GuidType, Information.ExtractPropertyPath<Subscription>(x => x.MessageType)), messageTypePk));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] subscriptions = _dataService.LoadObjects(lcs);
            if (subscriptionId != null)
            {
                subscriptions = subscriptions.Where(s => Guid.Parse(s.__PrimaryKey.ToString()) == Guid.Parse(subscriptionId)).ToArray();
            }

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.SubscribeOrUpdate() load Subscription.");

            Subscription subscription;
            if (subscriptions.Length == 0)
            {
                subscription = new Subscription()
                {
                    Client = ServiceHelper.GetClient(clientPk, _dataService, _statisticsService),
                    MessageType = ServiceHelper.GetMessageType(messageTypePk, _dataService, _statisticsService),
                    IsCallback = isCallback
                };

                if (subscriptionId != null)
                {
                    subscription.__PrimaryKey = Guid.Parse(subscriptionId);
                }

                if (isCallback)
                {
                    if (transportType == null)
                        throw new ArgumentException("Не указан способ передачи при создании callback-подписки.");

                    subscription.TransportType = transportType.Value;
                }
            }
            else
            {
                subscription = (Subscription)subscriptions[0];

                stopwatch = new Stopwatch();
                stopwatch.Start();

                _dataService.LoadObject(subscription);

                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.SubscribeOrUpdate() load Subscription 2.");

                subscription.IsCallback = isCallback;

                if (isCallback && transportType != null)
                    subscription.TransportType = transportType.Value;
            }

            if (expiryDate != null)
            {
                subscription.ExpiryDate = expiryDate.Value;
            }
            else
            {
                ServiceHelper.UpdateStoppingDate(subscription);
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();

            _dataService.UpdateObject(subscription);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.SubscribeOrUpdate() update Subscription.");
        }

        /// <summary>
        /// Обновить все подписки указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписки которого нужно обновить.</param>
        public void UpdateAllSubscriptions(string clientId)
        {
            Guid primaryKey = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, _dataService, _statisticsService);
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Subscription), Subscription.Views.SubscriptionsManagerView);
            lcs.LimitFunction = LangDef.GetFunction(LangDef.funcEQ, new VariableDef(LangDef.GuidType, "Client"), primaryKey);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var subscribes = _dataService.LoadObjects(lcs).Cast<Subscription>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DefaultSubscriptionsManager.UpdateAllSubscriptions() load Subscription.");

            foreach (var subscribe in subscribes)
            {
                SubscribeOrUpdate(clientId, subscribe.MessageType.ID, subscribe.IsCallback, null);
            }
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
    }
}
