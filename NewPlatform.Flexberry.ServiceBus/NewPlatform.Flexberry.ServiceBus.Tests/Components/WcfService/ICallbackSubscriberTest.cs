namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using ClientTools;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    public class WcfCallbackFixture : BaseServiceBusTest, IDisposable
    {
        private readonly WcfService service;

        public ICallbackSubscriber ServiceBus;
        public Mock<IReceivingManager> RecManager;

        public WcfCallbackFixture()
        {
            // Arrange.
            var address = new Uri("http://localhost:12346/CallbackSubscriber");
            RecManager = new Mock<IReceivingManager>();
            service = new WcfService(GetMockSubscriptionManager(), GetMockSendingManager(), RecManager.Object, GetMockLogger())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = address
            };

            var binding = new BasicHttpBinding();
            ServiceBus = new ChannelFactory<ICallbackSubscriber>(binding, new EndpointAddress(address)).CreateChannel();
            service.Start();
        }

        public void Dispose()
        {
            service.Stop();
            service.Dispose();
        }
    }

    [Collection("WcfServiceTests")]
    public class ICallbackSubscriberTest : IClassFixture<WcfCallbackFixture>
    {
        private readonly WcfCallbackFixture fixture;

        public ICallbackSubscriberTest(WcfCallbackFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Test for AcceptMessage method.
        /// </summary>
        [Fact]
        public void TestAcceptMessage()
        {
            // Arrange.
            const string clientId = "Client1";
            const string senderId = "Client2";
            const string messageTypeId = "messageType1";
            const string groupId = "Group1";
            var message = new MessageFromESB
            {
                GroupID = groupId,
                MessageTypeID = messageTypeId,
                Body = "Сообщение для шины",
                Tags = new Dictionary<string, string>(),
                SenderName = senderId
            };
            message.Tags["sendingWay"] = clientId;

            // Act & Assert.
            fixture.ServiceBus.AcceptMessage(message);
            fixture.RecManager.Verify(
                rec =>
                    rec.AcceptMessage(
                        It.Is<MessageForESB>(
                            msg =>
                                msg.ClientID == clientId && msg.MessageTypeID == messageTypeId &&
                                msg.Body == message.Body && msg.Tags["senderName"] == senderId), groupId),
                Times.Once);

            message.GroupID = string.Empty;
            fixture.ServiceBus.AcceptMessage(message);
            fixture.RecManager.Verify(
                rec =>
                    rec.AcceptMessage(
                        It.Is<MessageForESB>(
                            msg =>
                                msg.ClientID == clientId && msg.MessageTypeID == messageTypeId &&
                                msg.Body == message.Body && msg.Tags["senderName"] == senderId)),
                Times.Once);
        }

        /// <summary>
        /// Test for GetSourceId method.
        /// </summary>
        [Fact]
        public void TestGetSourceId()
        {
            // Act.
            var res = fixture.ServiceBus.GetSourceId();

            // Assert.
            Assert.Equal(res, ConfigurationManager.AppSettings["ServiceBusClientKey"]);
        }
    }
}
