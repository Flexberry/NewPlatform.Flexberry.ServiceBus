namespace NewPlatform.Flexberry.ServiceBus.RabbitMQTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Moq;
    using NewPlatform.Flexberry.ServiceBus.Components;
    using NewPlatform.Flexberry.ServiceBus.Components.StatisticsService;
    using RabbitMQ.Client.Exceptions;
    using Xunit;

    public class DefaultRmqComponentsTest : BaseRmqComponentsTest
    {
        public DefaultRmqComponentsTest()
            : base("RmqTest")
        {
        }

        [Fact]
        public void TestRouting()
        {
            string testVhost = ConfigurationManager.AppSettings["TestVhost"];
            var queues = managementClient.GetQueuesAsync().Result.Where(x => x.Vhost == testVhost).ToList();
            var exchanges = managementClient.GetExchangesAsync().Result.Where(x => x.Vhost == testVhost).ToList();
            var users = managementClient.GetUsersAsync().Result.ToList();
            var subs = rmqSubscriptionsManager.GetSubscriptions();

            Assert.Contains(users, x => x.Name == SenderId);
            Assert.Contains(exchanges, x => x.Name == amqpNamingManager.GetExchangeName(CallbackMsgTypeId));
            Assert.Contains(exchanges, x => x.Name == amqpNamingManager.GetExchangeName(WithoutCallbackMsgTypeId));
            Assert.Contains(queues, x => x.Name == amqpNamingManager.GetClientQueueName(CallbackReceiverId, CallbackMsgTypeId));
            Assert.Contains(queues, x => x.Name == amqpNamingManager.GetClientQueueName(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId));

            Assert.Contains(subs, sub => sub.Client.ID == WithoutCallbackReceiverId && sub.MessageType.ID == WithoutCallbackMsgTypeId);
            Assert.Contains(subs, sub => sub.Client.ID == CallbackReceiverId && sub.MessageType.ID == CallbackMsgTypeId);
        }

        [Fact]
        public void TestSendingWithoutPermission()
        {
            Assert.Throws<AlreadyClosedException>(() => rmqReceivingManager.AcceptMessage(new ServiceBusMessage()
            { ClientID = SenderWithoutPermissionsId, MessageTypeID = CallbackMsgTypeId }));
        }

        [Fact]
        public void TestBasicGet()
        {
            byte[] attachment = { 123, 12, 1, 2 };
            var tags = new Dictionary<string, string>()
                {
                    { "TestKey1", "TestValue1" },
                    { "TestKey2", "TestValue2" }
                };

            rmqReceivingManager.AcceptMessage(new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = WithoutCallbackMsgTypeId,
                Body = "TestBody",
                Attachment = attachment,
                Tags = tags
            });

            var message = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);
            rmqSendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            var emptyMessage = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);

            Assert.Equal(message.BinaryAttachment, attachment);
            Assert.Equal("TestBody", message.Body);
            Assert.Equal("TestKey1:TestValue1, TestKey2:TestValue2", message.Tags);
            Assert.Equal(message.Sender, SenderId);
            Assert.Equal(message.MessageType.ID, WithoutCallbackMsgTypeId);
            Assert.Null(emptyMessage);
        }

        [Fact]
        public void TestBasicGetWithGroup()
        {
            byte[] attachment = { 11, 11 };
            var tags = new Dictionary<string, string>()
            {
                { "TestKey11", "TestValue11" },
                { "TestKey22", "TestValue22" }
            };

            var sbMessage = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = WithoutCallbackMsgTypeId,
                Body = "TestBody2",
                Attachment = attachment,
                Tags = tags
            };
            rmqReceivingManager.AcceptMessage(sbMessage, "TestGroup");
            rmqReceivingManager.AcceptMessage(sbMessage, "TestGroup");

            var message = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);
            rmqSendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            var emptyMessage = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);

            Assert.Equal(message.BinaryAttachment, attachment);
            Assert.Equal("TestBody2", message.Body);
            Assert.Equal("TestKey11:TestValue11, TestKey22:TestValue22", message.Tags);
            Assert.Equal(message.Sender, SenderId);
            Assert.Equal(message.MessageType.ID, WithoutCallbackMsgTypeId);
            Assert.Null(emptyMessage);
        }

        [Fact]
        public void TestCallbackReceive()
        {
            byte[] attachment = { 11, 11 };
            var tags = new Dictionary<string, string>()
            {
                { "TestKey11", "TestValue11" },
                { "TestKey22", "TestValue22" }
            };

            var sbMessage1 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = CallbackMsgTypeId,
                Body = "ThrowException",
                Attachment = attachment,
                Tags = tags
            };

            var sbMessage2 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = CallbackMsgTypeId,
                Body = "TestBody2",
                Attachment = attachment,
                Tags = tags
            };

            rmqReceivingManager.AcceptMessage(sbMessage1);
            rmqReceivingManager.AcceptMessage(sbMessage2);

            var resetEvent = new ManualResetEvent(false);
            var client = new TestCallbackClient();
            client.ExpectedMessageCount = 2;
            client.ResetEvent = resetEvent;
            using (var host = new ServiceHost(client))
            {
                host.Open();
                rmqSendingManager.Start();
                resetEvent.WaitOne();

                var message = rmqSendingManager.ReadMessage(CallbackReceiverId, CallbackMsgTypeId);
                Assert.NotNull(message);
                rmqSendingManager.DeleteMessage(message.__PrimaryKey.ToString());
                var emptyMessage = rmqSendingManager.ReadMessage(CallbackReceiverId, CallbackMsgTypeId);
                Assert.Null(emptyMessage);
            }
        }

        [Fact]
        public void TestStatistics()
        {
            ClearStatistics();

            byte[] attachment = { 11, 11 };
            var tags = new Dictionary<string, string>()
            {
                { "TestKey11", "TestValue11" },
                { "TestKey22", "TestValue22" }
            };

            var callbackSbMessage1 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = CallbackMsgTypeId,
                Body = "TestBody1",
                Attachment = attachment,
                Tags = tags
            };

            var callbackSbMessage2 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = CallbackMsgTypeId,
                Body = "ThrowException",
                Attachment = attachment,
                Tags = tags
            };

            var withoutCallbackSbMessage1 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = WithoutCallbackMsgTypeId,
                Body = "TestBody1",
                Attachment = attachment,
                Tags = tags
            };

            var withoutCallbackSbMessage2 = new ServiceBusMessage()
            {
                ClientID = SenderId,
                MessageTypeID = WithoutCallbackMsgTypeId,
                Body = "TestBody2",
                Attachment = attachment,
                Tags = tags
            };

            rmqReceivingManager.AcceptMessage(callbackSbMessage1);
            rmqReceivingManager.AcceptMessage(callbackSbMessage2);
            rmqReceivingManager.AcceptMessage(withoutCallbackSbMessage1);
            rmqReceivingManager.AcceptMessage(withoutCallbackSbMessage2);

            var resetEvent = new ManualResetEvent(false);
            var client = new TestCallbackClient();
            client.ExpectedMessageCount = 2;
            client.ResetEvent = resetEvent;
            using (var host = new ServiceHost(client))
            {
                host.Open();
                rmqSendingManager.Start();
                resetEvent.WaitOne();

                var message = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);
                rmqSendingManager.DeleteMessage(message.__PrimaryKey.ToString());

                var info = new List<StatisticsRecord>();
                var mockStatisticsSetting = new Mock<IStatisticsSettings>();
                var mockStatisticsSaveService = new Mock<IStatisticsSaveService>();
                mockStatisticsSaveService.Setup(x => x.Save(It.IsAny<IEnumerable<StatisticsRecord>>())).Callback(
                    new Action<IEnumerable<StatisticsRecord>>(x => info = x.ToList()));

                string testVhost = ConfigurationManager.AppSettings["TestVhost"];
                var collector = new RmqStatisticsCollector(
                    GetMockLogger(),
                    esbSubscriptionsManager,
                    mockStatisticsSetting.Object,
                    managementClient,
                    amqpNamingManager,
                    mockStatisticsSaveService.Object,
                    testVhost);

                collector.Prepare();
                MethodInfo collectMethod = collector.GetType().GetMethod("TimerCallBack", BindingFlags.NonPublic | BindingFlags.Instance);
                collectMethod.Invoke(collector, new[] { new object() });

                Assert.Contains(info, x => x.SentCount == 2
                    && x.ReceivedCount == 2
                    && x.QueueLength == 0
                    && x.StatisticsSetting.Subscription.Client.ID == WithoutCallbackReceiverId
                    && x.StatisticsSetting.Subscription.MessageType.ID == WithoutCallbackMsgTypeId);

                Assert.Contains(info, x => x.SentCount == 2
                    && x.ReceivedCount == 1
                    && x.QueueLength == 0
                    && x.StatisticsSetting.Subscription.Client.ID == CallbackReceiverId
                    && x.StatisticsSetting.Subscription.MessageType.ID == CallbackMsgTypeId);

                client.ResetMessageCount();
                client.ResetEvent.Reset();
                rmqReceivingManager.AcceptMessage(callbackSbMessage1);
                rmqReceivingManager.AcceptMessage(callbackSbMessage2);
                rmqReceivingManager.AcceptMessage(withoutCallbackSbMessage1);
                rmqReceivingManager.AcceptMessage(withoutCallbackSbMessage2);
                resetEvent.WaitOne();

                var message2 = rmqSendingManager.ReadMessage(WithoutCallbackReceiverId, WithoutCallbackMsgTypeId);
                rmqSendingManager.DeleteMessage(message.__PrimaryKey.ToString());

                Assert.Contains(info, x => x.SentCount == 2
                    && x.ReceivedCount == 2
                    && x.QueueLength == 0
                    && x.StatisticsSetting.Subscription.Client.ID == WithoutCallbackReceiverId
                    && x.StatisticsSetting.Subscription.MessageType.ID == WithoutCallbackMsgTypeId);

                Assert.Contains(info, x => x.SentCount == 2
                    && x.ReceivedCount == 1
                    && x.QueueLength == 0
                    && x.StatisticsSetting.Subscription.Client.ID == CallbackReceiverId
                    && x.StatisticsSetting.Subscription.MessageType.ID == CallbackMsgTypeId);
            }
        }

        private void ClearStatistics()
        {
            string userName = ConfigurationManager.AppSettings["DefaultRmqUserName"];
            string password = ConfigurationManager.AppSettings["DefaultRmqUserPassword"];
            string host = ConfigurationManager.AppSettings["DefaultRmqHost"];
            string managementPort = ConfigurationManager.AppSettings["DefaultRmqManagementPort"];

            string sURL = $"http://{host}:{managementPort}/api/reset";
            var request = WebRequest.Create(sURL);
            request.Method = "DELETE";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
            var response = request.GetResponse();
        }
    }
}
