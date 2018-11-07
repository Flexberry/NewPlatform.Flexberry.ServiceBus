namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Linq;
    using System.Threading;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    using Microsoft.Owin.Hosting;

    using NewPlatform.Flexberry.ServiceBus.Components;
    using Owin;
    using Xunit;

    public class OptimizedSendingManagerTest : BaseServiceBusIntegratedTest
    {
        public OptimizedSendingManagerTest()
            : base("testOSM")
        {
        }

        [Fact]
        public void TestDeleteUndeliveredMessageWithGroup()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var messageType = new MessageType() { ID = "messageTypeId" };
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId", Address = "http://localhost:2525/Message" };
                var messageWithoutSubscription = new Message() { Recipient = sender, MessageType = messageType, SendingTime = DateTime.Now, Group = "group" };
                var firstMessage = new Message() { Recipient = recipient, MessageType = messageType, SendingTime = DateTime.Now, Group = "group" };
                var lastMessage = new Message() { Recipient = recipient, MessageType = messageType, SendingTime = DateTime.Now.AddSeconds(1), Group = "group" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType, IsCallback = true, TransportType = TransportType.HTTP, ExpiryDate = DateTime.Now.AddDays(1) };
                var dataObjects = new DataObject[] { messageType, sender, recipient, messageWithoutSubscription, firstMessage, lastMessage, subscription };

                dataService.UpdateObjects(ref dataObjects);

                var statisticsService = GetMockStatisticsService();
                var component = new OptimizedSendingManager(
                    new DefaultSubscriptionsManager(dataService, statisticsService),
                    statisticsService,
                    dataService,
                    GetMockLogger())
                {
                    MaxTasks = 1,
                    ScanningPeriodMilliseconds = 5000
                };

                // Act.
                using (WebApp.Start("http://localhost:2525/Message", builder => builder.Run(context => throw new Exception())))
                {
                    component.Prepare();
                    component.Start();
                    Thread.Sleep(component.ScanningPeriodMilliseconds);
                    component.Stop();
                    component.AfterStop();
                }

                // Assert.
                Assert.Equal(2, ((SQLDataService)dataService).Query<Message>().Count());
            }
        }
    }
}
