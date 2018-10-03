namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.ServiceModel;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    public class WcfInteropFixture : BaseServiceBusTest, IDisposable
    {
        private readonly WcfService service;

        public IServiceBusInterop ServiceBus;
        public Mock<ISubscriptionsManager> SubManager;
        public NameCommentStruct TestNCS;

        public WcfInteropFixture()
        {
            // Arrange.
            TestNCS = new NameCommentStruct { Comment = "Comm", Id = "Test", Name = "Name" };
            var address = new Uri("http://localhost:12344/ServiceBusInterop");
            SubManager = new Mock<ISubscriptionsManager>();
            SubManager.Setup(sub => sub.GetSubscriptions(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[]
                {
                    new Subscription()
                    {
                        MessageType = new MessageType() { ID = TestNCS.Id, Description = TestNCS.Comment, Name = TestNCS.Name }
                    }
                });
            service = new WcfService(SubManager.Object, GetMockSendingManager(), GetMockReceivingManager(), GetMockLogger(), GetMockStatisticsService())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = address
            };

            var binding = new BasicHttpBinding();
            ServiceBus = new ChannelFactory<IServiceBusInterop>(binding, new EndpointAddress(address)).CreateChannel();
            service.Start();
        }

        public void Dispose()
        {
            service.Stop();
            service.Dispose();
        }
    }

    [Collection("WcfServiceTests")]
    public class IServiceBusInteropTest : IClassFixture<WcfInteropFixture>
    {
        private readonly WcfInteropFixture fixture;

        public IServiceBusInteropTest(WcfInteropFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Test for AddNewEvntType method.
        /// </summary>
        [Fact]
        public void TestAddNewEvntType()
        {
            // Arrange.
            var ncstruct = new NameCommentStruct { Id = "TestStruct1", Comment = "123" };

            // Act.
            fixture.ServiceBus.AddNewEvntType(ncstruct);

            // Assert.
            fixture.SubManager.Verify(
                sub =>
                    sub.CreateEventType(
                        It.Is<NameCommentStruct>(
                            ncs => ncs.Id == ncstruct.Id && ncs.Comment == ncstruct.Comment && ncs.Name == ncstruct.Name)),
                Times.Once);
        }

        /// <summary>
        /// Test for AddNewMsgType method.
        /// </summary>
        [Fact]
        public void TestAddNewMsgType()
        {
            // Arrange.
            var ncstruct = new NameCommentStruct { Id = "TestStruct2", Comment = "Comm", Name = "NCS" };

            // Act.
            fixture.ServiceBus.AddNewMsgType(ncstruct);

            // Assert.
            fixture.SubManager.Verify(
                sub =>
                    sub.CreateMessageType(
                        It.Is<NameCommentStruct>(
                            ncs => ncs.Id == ncstruct.Id && ncs.Comment == ncstruct.Comment && ncs.Name == ncstruct.Name)),
                Times.Once);
        }

        /// <summary>
        /// Test for GetEvntTypesFromBus method.
        /// </summary>
        [Fact]
        public void TestGetEvntTypesFromBus()
        {
            // Arrange.
            const string clientId = "TestClient1";

            // Act.
            var result = fixture.ServiceBus.GetEvntTypesFromBus(clientId);

            // Assert.
            Assert.Equal(result.Length, 1);
            Assert.True(result[0].Id == fixture.TestNCS.Id && result[0].Comment == fixture.TestNCS.Comment && result[0].Name == fixture.TestNCS.Name);
            fixture.SubManager.Verify(sub => sub.GetSubscriptions(clientId, true), Times.Once);
        }

        /// <summary>
        /// Test for GetMsgTypesFromBus method.
        /// </summary>
        [Fact]
        public void TestGetMsgTypesFromBus()
        {
            // Arrange.
            const string clientId = "TestClient2";

            // Act.
            var result = fixture.ServiceBus.GetMsgTypesFromBus(clientId);

            // Assert.
            Assert.Equal(result.Length, 1);
            Assert.True(result[0].Id == fixture.TestNCS.Id && result[0].Comment == fixture.TestNCS.Comment && result[0].Name == fixture.TestNCS.Name);
            fixture.SubManager.Verify(sub => sub.GetSubscriptions(clientId, true), Times.Once);
        }

        /// <summary>
        /// Test for UpdateClientSubscribesForEvnts method.
        /// </summary>
        [Fact]
        public void TestUpdateClientSubscribesForEvnts()
        {
            // Arrange.
            const string clientId = "TestClient3";

            // Act.
            fixture.ServiceBus.UpdateClientSubscribesForEvnts(clientId);

            // Assert.
            fixture.SubManager.Verify(sub => sub.UpdateAllSubscriptions(clientId), Times.Once);
        }

        /// <summary>
        /// Test for UpdateClientSubscribesForMsgs method.
        /// </summary>
        [Fact]
        public void TestUpdateClientSubscribesForMsgs()
        {
            // Arrange.
            const string clientId = "TestClient4";

            // Act.
            fixture.ServiceBus.UpdateClientSubscribesForMsgs(clientId);

            // Assert.
            fixture.SubManager.Verify(sub => sub.UpdateAllSubscriptions(clientId), Times.Once);
        }
    }
}
