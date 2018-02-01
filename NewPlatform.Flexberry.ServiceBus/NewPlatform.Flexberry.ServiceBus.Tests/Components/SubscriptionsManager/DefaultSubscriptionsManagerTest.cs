namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultSubscriptionsManager component.
    /// </summary>
    public class DefaultSubscriptionsManagerTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultSubscriptionsManager component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new DefaultSubscriptionsManager(GetMockDataService(), GetMockStatisticsService());

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Testing message type creation.
        /// </summary>
        [Fact]
        public void TestMessageTypeCreate()
        {
            // Arrange.
            var messageTypeInfo = new NameCommentStruct { Id = "123", Name = "TestMessageType", Comment = "ForTest" };
            var dataServiceMock = new Mock<IDataService>();
            var service = new DefaultSubscriptionsManager(dataServiceMock.Object, GetMockStatisticsService());

            // Act.
            service.CreateMessageType(messageTypeInfo);

            // Assert.
            dataServiceMock.Verify(f => f.UpdateObject(It.Is<MessageType>(t => t.ID == messageTypeInfo.Id && t.Name == messageTypeInfo.Name && t.Description == messageTypeInfo.Comment)), Times.Once);
        }

        /// <summary>
        /// Testing event type creation.
        /// </summary>
        [Fact]
        public void TestEventTypeCreate()
        {
            // Arrange.
            var eventTypeInfo = new NameCommentStruct { Id = "123", Name = "TestEventType", Comment = "ForTest" };
            var dataServiceMock = new Mock<IDataService>();
            var service = new DefaultSubscriptionsManager(dataServiceMock.Object, GetMockStatisticsService());

            // Act.
            service.CreateEventType(eventTypeInfo);

            // Assert.
            dataServiceMock.Verify(f => f.UpdateObject(It.Is<MessageType>(t => t.ID == eventTypeInfo.Id && t.Name == eventTypeInfo.Name && t.Description == eventTypeInfo.Comment)), Times.Once);
        }

        /// <summary>
        /// Testing client creation.
        /// </summary>
        [Fact]
        public void TestClientCreate()
        {
            // Arrange.
            const string clientId = "03FE3B98-2D09-4032-A5BF-03BEDF86F4F4";
            const string clientName = "SucessClient";
            const string clientAddress = "TestAddress";
            var dataServiceMock = new Mock<IDataService>();
            var service = new DefaultSubscriptionsManager(dataServiceMock.Object, GetMockStatisticsService());

            // Act.
            service.CreateClient(clientId, clientName, clientAddress);

            // Assert.
            dataServiceMock.Verify(f => f.UpdateObject(It.Is<Client>(t => t.ID == clientId && t.Name == clientName && t.Address == clientAddress)), Times.Once);
        }
    }
}
