namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultReceivingManager component.
    /// </summary>
    public class DefaultReceivingManagerTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultReceivingManager component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new DefaultReceivingManager(
                GetMockLogger(),
                GetMockObjectRepository(),
                GetMockSubscriptionManager(),
                GetMockSendingManager(),
                GetMockDataService(),
                GetMockStatisticsService());

            RunSBComponentFullCycle(service);
        }

        [Fact]
        public void TestAcceptMessageWithoutRestriction()
        {
            // Arrange.
            var sender = new Client() { ID = "senderId" };
            var recipient = new Client() { ID = "recipientId" };
            var messageType = new MessageType() { ID = "messageTypeId" };
            var secondMessageType = new MessageType() { ID = "secondMessageTypeId" };
            var mockLogger = new Mock<ILogger>();
            var mockObjectRepository = new Mock<IObjectRepository>();

            mockObjectRepository
                .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                .Returns(new[] { new SendingPermission() { Client = sender, MessageType = secondMessageType } });

            var component = new DefaultReceivingManager(
                mockLogger.Object,
                mockObjectRepository.Object,
                GetMockSubscriptionManager(),
                GetMockSendingManager(),
                GetMockDataService(),
                GetMockStatisticsService());
            var messageForESB = new MessageForESB()
            {
                ClientID = sender.ID,
                MessageTypeID = messageType.ID,
            };

            // Act.
            component.AcceptMessage(messageForESB);

            // Assert.
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(title => title == "Отправка запрещена."),
                    It.Is<string>(message => message == $"Клиент {sender.ID} не имеет прав на отправку сообщения типа {messageType.ID}.")),
                Times.Once);
        }

        [Fact]
        public void TestAcceptMessageWithoutSubscription()
        {
            // Arrange.
            var sender = new Client() { ID = "senderId" };
            var recipient = new Client() { ID = "recipientId" };
            var messageType = new MessageType() { ID = "messageTypeId" };
            var subscription = new Subscription() { Client = recipient, MessageType = messageType };
            var mockLogger = new Mock<ILogger>();
            var mockObjectRepository = new Mock<IObjectRepository>();
            var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
            mockObjectRepository
                .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
            mockSubscriptionManager
                .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                .Returns(new Subscription[] { });

            var component = new DefaultReceivingManager(
                mockLogger.Object,
                mockObjectRepository.Object,
                mockSubscriptionManager.Object,
                GetMockSendingManager(),
                GetMockDataService(),
                GetMockStatisticsService());
            var messageForESB = new MessageForESB()
            {
                ClientID = sender.ID,
                MessageTypeID = messageType.ID,
            };

            // Act.
            component.AcceptMessage(messageForESB);

            // Assert.
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(title => title == "Для сообщения нет ни одной подписки."),
                    It.Is<string>(message => message == $"Было получено сообщение, для которого нет ни одной активной подписки (ID типа сообщения: {messageType.ID}).")),
                Times.Once);
        }
    }
}