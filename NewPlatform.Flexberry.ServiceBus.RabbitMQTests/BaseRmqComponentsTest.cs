namespace NewPlatform.Flexberry.ServiceBus.RabbitMQTests
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using EasyNetQ.Management.Client;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using NewPlatform.Flexberry.ServiceBus.Components;
    using NewPlatform.Flexberry.ServiceBus.IntegratedTests;
    using RabbitMQ.Client;

    public class BaseRmqComponentsTest : BaseServiceBusIntegratedTest
    {
        protected const string CallbackMsgTypeId = "CallbackMsgTypeId";
        protected const string WithoutCallbackMsgTypeId = "WithoutCallbackMsgTypeId";

        protected const string SenderId = "SenderId";
        protected const string SenderWithoutPermissionsId = "SenderWithoutPermissionsId";
        protected const string CallbackReceiverId = "CallbackReceiverId";
        protected const string WithoutCallbackReceiverId = "WithoutCallbackReceiverId";

        protected IDataService DataService;
        protected ManagementClient managementClient;
        protected ConnectionFactory connectionFactory;
        protected AmqpNamingManager amqpNamingManager;
        internal DefaultSubscriptionsManager esbSubscriptionsManager;
        internal RmqSubscriptionsManager rmqSubscriptionsManager;
        internal RmqSubscriptionsSynchronizer rmqSubscriptionsSynchronizer;
        internal RmqMessageConverter rmqMessageConverter;
        internal RmqReceivingManager rmqReceivingManager;
        internal RmqSendingManager rmqSendingManager;

        public BaseRmqComponentsTest(string tmpDbNamePrefix)
            : base(tmpDbNamePrefix)
        {
            DataService = DataServices.First();
            InitializeDatabase();
            InitializeComponents();
            PrepareComponents();
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            MethodInfo syncMethod = rmqSubscriptionsSynchronizer.GetType().GetMethod("Sync", BindingFlags.NonPublic | BindingFlags.Instance);
            syncMethod.Invoke(rmqSubscriptionsSynchronizer, new object[] { });

            var vHost = managementClient.GetVhostAsync(ConfigurationManager.AppSettings["TestVhost"]).Result;
            var callbackQueue = managementClient.GetQueueAsync(amqpNamingManager.GetClientQueueName(CallbackReceiverId, CallbackMsgTypeId), vHost).Result;
            var withoutCallbackQueue = managementClient.GetQueueAsync(amqpNamingManager.GetClientQueueName(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId), vHost).Result;
            managementClient.PurgeAsync(callbackQueue).Wait();
            managementClient.PurgeAsync(withoutCallbackQueue).Wait();
        }

        private void InitializeDatabase()
        {
            var sender = new Client() { ID = SenderId, Name = "Sender", Description = "TestSender" };
            var senderWithoutPermissions = new Client() { ID = SenderWithoutPermissionsId, Name = "SenderWithoutPermissions", Description = "TestSenderWithoutPermissions" };
            var receiverWithCallback = new Client() { ID = CallbackReceiverId, Name = "CallbackReceiver", Description = "TestCallbackReceiver", Address = "http://localhost:12345/SbListener" };
            var receiverWithoutCallback = new Client() { ID = WithoutCallbackReceiverId, Name = "WithoutCallbackReceiver", Description = "TestWithoutCallbackReceiver" };

            var callbackMessageType = new MessageType() { ID = CallbackMsgTypeId, Name = "CallbackMsgType" };
            var withoutCallbackMessageType = new MessageType() { ID = WithoutCallbackMsgTypeId, Name = "WithoutCallbackMsgType" };

            var sendingPermission1 = new SendingPermission() { Client = sender, MessageType = callbackMessageType };
            var sendingPermission2 = new SendingPermission() { Client = sender, MessageType = withoutCallbackMessageType };

            DataObject[] objsToUpdate = { sender, senderWithoutPermissions, receiverWithCallback, receiverWithoutCallback, sendingPermission1, sendingPermission2 };
            DataService.UpdateObjects(ref objsToUpdate);

            var esbSubscriptionsManager = new DefaultSubscriptionsManager(DataService, GetMockStatisticsService());
            esbSubscriptionsManager.SubscribeOrUpdate(CallbackReceiverId, CallbackMsgTypeId, true, TransportType.WCF, new DateTime(2020, 2, 2));
            esbSubscriptionsManager.SubscribeOrUpdate(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId, false, TransportType.WCF, new DateTime(2020, 2, 2));
        }

        private void InitializeComponents()
        {
            string hostName = ConfigurationManager.AppSettings["DefaultRmqHost"];
            string userName = ConfigurationManager.AppSettings["DefaultRmqUserName"];
            string password = ConfigurationManager.AppSettings["DefaultRmqUserPassword"];
            string testVhost = ConfigurationManager.AppSettings["TestVhost"];
            string amqpPort = ConfigurationManager.AppSettings["DefaultRmqAmqpPort"];
            string testRmqUri = $"amqp://{userName}:{password}@{hostName}:{amqpPort}/{testVhost}";

            managementClient = new ManagementClient(hostName, userName, password);
            connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                VirtualHost = testVhost,
                DispatchConsumersAsync = true
            };
            amqpNamingManager = new AmqpNamingManager();
            rmqMessageConverter = new RmqMessageConverter();

            esbSubscriptionsManager = new DefaultSubscriptionsManager(DataService, GetMockStatisticsService());
            rmqSubscriptionsManager = new RmqSubscriptionsManager(GetMockLogger(), managementClient, testVhost);
            rmqSubscriptionsSynchronizer = new RmqSubscriptionsSynchronizer(
                GetMockLogger(), esbSubscriptionsManager, rmqSubscriptionsManager, DataService, managementClient, testVhost);
            rmqReceivingManager = new RmqReceivingManager(GetMockLogger(), rmqMessageConverter, 
                new Uri(testRmqUri));
            rmqReceivingManager.RmqVirtualHost = testVhost;
            rmqSendingManager = new RmqSendingManager(GetMockLogger(), esbSubscriptionsManager, connectionFactory,
                managementClient, rmqMessageConverter, amqpNamingManager, testVhost);
        }

        private void PrepareComponents()
        {
            esbSubscriptionsManager.Prepare();
            rmqSubscriptionsManager.Prepare();
            rmqSubscriptionsSynchronizer.Prepare();
            rmqReceivingManager.Prepare();
            rmqSendingManager.Prepare();
        }
    }
}
