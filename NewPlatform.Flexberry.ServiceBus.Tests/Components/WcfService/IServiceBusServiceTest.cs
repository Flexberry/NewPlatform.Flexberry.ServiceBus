namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.Text;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    public class WcfServiceFixture : BaseServiceBusTest, IDisposable
    {
        private readonly WcfService service;

        public IServiceBusService ServiceBus;
        public Mock<ISubscriptionsManager> SubManager;
        public Mock<ISendingManager> SendManager;
        public Mock<IReceivingManager> RecManager;

        public WcfServiceFixture()
        {
            // Arrange.
            var address = new Uri("http://localhost:12345/ServiceBusService");
            SubManager = new Mock<ISubscriptionsManager>();
            SendManager = new Mock<ISendingManager>();
            RecManager = new Mock<IReceivingManager>();
            service = new WcfService(SubManager.Object, SendManager.Object, RecManager.Object, GetMockLogger())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = address
            };

            var binding = new BasicHttpBinding();
            ServiceBus = new ChannelFactory<IServiceBusService>(binding, new EndpointAddress(address)).CreateChannel();
            service.Start();
        }

        public void Dispose()
        {
            service.Stop();
            service.Dispose();
        }
    }

    [Collection("WcfServiceTests")]
    public class IServiceBusServiceTest : IClassFixture<WcfServiceFixture>
    {
        private readonly WcfServiceFixture fixture;

        public IServiceBusServiceTest(WcfServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Test for CreateClient method.
        /// </summary>
        [Fact]
        public void TestCreateClient()
        {
            // Arrange.
            const string clientId = "Client1";
            const string clientName = "Вася";
            const string clientAdress = "pupkin@ics.perm.ru";

            // Act.
            fixture.ServiceBus.CreateClient(clientId, clientName, clientAdress);

            // Assert.
            fixture.SubManager.Verify(sub => sub.CreateClient(clientId, clientName, clientAdress), Times.Once);
        }

        /// <summary>
        /// Test for DeleteClient method.
        /// </summary>
        [Fact]
        public void TestDeleteClient()
        {
            // Arrange.
            const string clientId = "Client2";

            // Act.
            fixture.ServiceBus.DeleteClient(clientId);

            // Assert.
            fixture.SubManager.Verify(sub => sub.DeleteClient(clientId), Times.Once);
        }

        /// <summary>
        /// Test for DoesEventRisen method.
        /// </summary>
        [Fact]
        public void TestDoesEventRisen()
        {
            // Arrange.
            const string clientId = "Client3";
            const string eventId = "Event1";
            fixture.SendManager.Setup(send => send.CheckEventIsRaised(clientId, eventId)).Returns(true);

            // Act.
            var res = fixture.ServiceBus.DoesEventRisen(clientId, eventId);

            // Assert.
            fixture.SendManager.Verify(send => send.CheckEventIsRaised(clientId, eventId), Times.Once);
            Assert.True(res);
        }

        /// <summary>
        /// Test for GetCurrentMessageCount method.
        /// </summary>
        [Fact]
        public void TestGetCurrentMessageCount()
        {
            // Arrange.
            const string clientId = "Client4";
            const int count = 5;
            fixture.SendManager.Setup(send => send.GetCurrentMessageCount(clientId)).Returns(count);

            // Act.
            var res = fixture.ServiceBus.GetCurrentMessageCount(clientId);

            // Assert.
            fixture.SendManager.Verify(send => send.GetCurrentMessageCount(clientId), Times.Once);
            Assert.Equal(res, count);
        }

        /// <summary>
        /// Test for GetCurrentThisTypeMessageCount method.
        /// </summary>
        [Fact]
        public void TestGetCurrentThisTypeMessageCount()
        {
            // Arrange.
            const string clientId = "Client5";
            const string messageTypeId = "messageType1";
            const int count = 10;
            fixture.SendManager.Setup(send => send.GetCurrentMessageCount(clientId, messageTypeId)).Returns(count);

            // Act.
            var res = fixture.ServiceBus.GetCurrentThisTypeMessageCount(clientId, messageTypeId);

            // Assert.
            fixture.SendManager.Verify(send => send.GetCurrentMessageCount(clientId, messageTypeId), Times.Once);
            Assert.Equal(res, count);
        }

        /// <summary>
        /// Test for GetMessageFromESB method.
        /// </summary>
        [Fact]
        public void TestGetMessageFromESB()
        {
            // Arrange.
            const string clientId = "Client6";
            const string messageTypeId = "messageType2";
            const string messageId = "0061BF80-3C74-46C6-92DD-F1DD3E584309";
            var attachment = Encoding.Unicode.GetBytes(clientId);
            var message = new Message()
            {
                __PrimaryKey = Guid.Parse(messageId),
                ReceivingTime = DateTime.Now,
                MessageType = new MessageType() { ID = messageTypeId },
                Body = "Какой-то текст",
                Sender = "Паша",
                Group = "Group1",
                BinaryAttachment = attachment,
                Tags = { }
            };
            fixture.SendManager.Setup(send => send.ReadMessage(clientId, messageTypeId)).Returns(message);

            // Act.
            var res = fixture.ServiceBus.GetMessageFromESB(clientId, messageTypeId);

            // Assert.
            fixture.SendManager.Verify(send => send.DeleteMessage(message.__PrimaryKey.ToString()), Times.Once);
            Assert.True(res.Body == message.Body && res.GroupID == message.Group &&
                res.MessageFormingTime == message.ReceivingTime && res.MessageTypeID == message.MessageType.ID &&
                res.SenderName == message.Sender && res.Tags["sendingWay"] == ConfigurationManager.AppSettings.Get("ServiceID4SB"));
        }

        /// <summary>
        /// Test for GetMessageInfo method.
        /// </summary>
        [Fact]
        public void TestGetMessageInfo()
        {
            // Arrange.
            const string clientId = "Client7";
            const string messageTypeId = "messageType3";
            const int priority = 5;
            var currentTime = DateTime.Now;
            fixture.SendManager.Setup(send => send.GetMessagesInfo(clientId, messageTypeId, It.IsAny<int>())).Returns(new[] { new MessageInfoFromESB { Priority = priority, MessageFormingTime = currentTime } });

            // Act.
            var res = fixture.ServiceBus.GetMessageInfo(clientId, messageTypeId);

            // Assert.
            Assert.True(res.FormingTime == currentTime && res.Priority == priority);
        }

        /// <summary>
        /// Test for GetMessageWithGroupFromESB method.
        /// </summary>
        [Fact]
        public void TestGetMessageWithGroupFromESB()
        {
            // Arrange.
            const string clientId = "Client8";
            const string messageTypeId = "messageType4";
            const string messageId = "9BA95036-6B4F-409C-A45E-C14895FAD7EE";
            var attachment = Encoding.Unicode.GetBytes(clientId);
            var message = new Message()
            {
                __PrimaryKey = Guid.Parse(messageId),
                ReceivingTime = DateTime.Now,
                MessageType = new MessageType() { ID = messageTypeId },
                Body = "Какой-то текст",
                Sender = "Паша",
                Group = "Group2",
                BinaryAttachment = attachment,
                Tags = { }
            };
            fixture.SendManager.Setup(send => send.ReadMessage(clientId, messageTypeId, message.Group)).Returns(message);

            // Act.
            var res = fixture.ServiceBus.GetMessageWithGroupFromESB(clientId, messageTypeId, message.Group);

            // Assert.
            fixture.SendManager.Verify(send => send.DeleteMessage(message.__PrimaryKey.ToString()), Times.Once);
            Assert.True(res.Body == message.Body && res.GroupID == message.Group &&
                res.MessageFormingTime == message.ReceivingTime && res.MessageTypeID == message.MessageType.ID &&
                res.SenderName == message.Sender && res.Tags["sendingWay"] == ConfigurationManager.AppSettings.Get("ServiceID4SB"));
        }

        /// <summary>
        /// Test for GetMessageInfoWithGroup method.
        /// </summary>
        [Fact]
        public void TestGetMessageInfoWithGroup()
        {
            // Arrange.
            const string clientId = "Client9";
            const string messageTypeId = "messageType5";
            const string groupId = "Group3";
            const int priority = 4;
            var currentTime = DateTime.Now;
            fixture.SendManager.Setup(send => send.GetMessagesInfo(clientId, messageTypeId, groupId, It.IsAny<int>())).Returns(new[] { new MessageInfoFromESB { Priority = priority, MessageFormingTime = currentTime } });

            // Act.
            var res = fixture.ServiceBus.GetMessageInfoWithGroup(clientId, messageTypeId, groupId);

            // Assert.
            Assert.True(res.FormingTime == currentTime && res.Priority == priority);
        }

        /// <summary>
        /// Test for GetMessageWithTagsFromESB method.
        /// </summary>
        [Fact]
        public void TestGetMessageWithTagsFromESB()
        {
            // Arrange.
            const string clientId = "Client10";
            const string messageTypeId = "messageType6";
            const string messageId = "F70FFA1D-E383-46AD-8F1C-755B31D18F4E";
            var attachment = Encoding.Unicode.GetBytes(clientId);
            var message = new Message
            {
                __PrimaryKey = Guid.Parse(messageId),
                ReceivingTime = DateTime.Now,
                MessageType = new MessageType() { ID = messageTypeId },
                Body = "Какой-то текст",
                Sender = "Паша",
                Group = "Group4",
                BinaryAttachment = attachment,
                Tags = { }
            };
            fixture.SendManager.Setup(send => send.ReadMessage(clientId, messageTypeId, new string[] { })).Returns(message);

            // Act.
            var res = fixture.ServiceBus.GetMessageWithTagsFromESB(clientId, messageTypeId, new string[] { });

            // Assert.
            fixture.SendManager.Verify(send => send.DeleteMessage(message.__PrimaryKey.ToString()), Times.Once);
            Assert.True(res.Body == message.Body && res.GroupID == message.Group &&
                res.MessageFormingTime == message.ReceivingTime && res.MessageTypeID == message.MessageType.ID &&
                res.SenderName == message.Sender && res.Tags["sendingWay"] == ConfigurationManager.AppSettings.Get("ServiceID4SB"));
        }

        /// <summary>
        /// Test for GetMessageInfoWithTags method.
        /// </summary>
        [Fact]
        public void TestGetMessageInfoWithTags()
        {
            // Arrange.
            const string clientId = "Client11";
            const string messageTypeId = "messageType7";
            const int priority = 3;
            var currentTime = DateTime.Now;
            fixture.SendManager.Setup(send => send.GetMessagesInfo(clientId, messageTypeId, new string[] { }, It.IsAny<int>())).Returns(new[] { new MessageInfoFromESB { Priority = priority, MessageFormingTime = currentTime } });

            // Act.
            var res = fixture.ServiceBus.GetMessageInfoWithTags(clientId, messageTypeId, new string[] { });

            // Assert.
            Assert.True(res.FormingTime == currentTime && res.Priority == priority);
        }

        /// <summary>
        /// Test for RiseEventOnESB method.
        /// </summary>
        [Fact]
        public void TestRiseEventOnESB()
        {
            // Arrange.
            const string clientId = "Client12";
            const string eventTypeId = "eventType1";

            // Act.
            fixture.ServiceBus.RiseEventOnESB(clientId, eventTypeId);

            // Assert.
            fixture.RecManager.Verify(rec => rec.RaiseEvent(clientId, eventTypeId), Times.Once);
        }

        /// <summary>
        /// Test for SendMessageToESB method.
        /// </summary>
        [Fact]
        public void TestSendMessageToESB()
        {
            // Arrange.
            const string clientId = "Client13";
            const string messageTypeId = "messageType8";
            var message = new MessageForESB { ClientID = clientId, MessageTypeID = messageTypeId };

            // Act.
            fixture.ServiceBus.SendMessageToESB(message);

            // Assert.
            fixture.RecManager.Verify(
                rec =>
                    rec.AcceptMessage(
                        It.Is<MessageForESB>(mes => mes.ClientID == clientId && mes.MessageTypeID == messageTypeId)),
                Times.Once);
        }

        /// <summary>
        /// Test for SendMessageToESBWithUseGroup method.
        /// </summary>
        [Fact]
        public void TestSendMessageToESBWithUseGroup()
        {
            // Arrange.
            const string clientId = "Client14";
            const string messageTypeId = "messageType9";
            const string groupId = "Group5";
            var message = new MessageForESB { ClientID = clientId, MessageTypeID = messageTypeId };

            // Act.
            fixture.ServiceBus.SendMessageToESBWithUseGroup(message, groupId);

            // Assert.
            fixture.RecManager.Verify(
                rec =>
                    rec.AcceptMessage(
                        It.Is<MessageForESB>(mes => mes.ClientID == clientId && mes.MessageTypeID == messageTypeId), groupId),
                Times.Once);
        }

        /// <summary>
        /// Test for SubscribeClientForEventCallback method.
        /// </summary>
        [Fact]
        public void TestSubscribeClientForEventCallback()
        {
            // Arrange.
            const string clientId = "Client15";
            const string eventTypeId = "eventType2";

            // Act.
            fixture.ServiceBus.SubscribeClientForEventCallback(clientId, eventTypeId);

            // Assert.
            fixture.SubManager.Verify(sub => sub.SubscribeOrUpdate(clientId, eventTypeId, true, TransportType.WCF, null), Times.Once);
        }

        /// <summary>
        /// Test for SubscribeClientForMessageCallback method.
        /// </summary>
        [Fact]
        public void TestSubscribeClientForMessageCallback()
        {
            // Arrange.
            const string clientId = "Client16";
            const string messageTypeId = "messageType10";

            // Act.
            fixture.ServiceBus.SubscribeClientForMessageCallback(clientId, messageTypeId);

            // Assert.
            fixture.SubManager.Verify(sub => sub.SubscribeOrUpdate(clientId, messageTypeId, true, TransportType.WCF, null), Times.Once);
        }
    }
}
