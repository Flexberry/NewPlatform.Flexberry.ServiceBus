namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    public class DatabaseFixture : BaseServiceBusIntegratedTest
    {
        // Arrange.
        public const string ClientId = "FDF33DF1-5DCA-41F9-A2E4-3B5C7E103452";
        public const string ServiceBusId = "31D12F7D-2D0E-43FB-8092-E6D34A9AB87D";
        public const string MessageTypeId = "EB6EC229-5E93-4B76-9993-5A1589787421";
        public const string ClientName = "ClientName";
        public const string ServiceBusName = "ServiceBusName";
        public const string MessageTypeName = "MessageTypeName";
        public IEnumerable<IDataService> TestDataServices;

        public DatabaseFixture()
            : base("HelperTests")
        {
            TestDataServices = DataServices;
            foreach (var dataservice in TestDataServices)
            {
                DataObject client = new Client() { __PrimaryKey = Guid.Parse(ClientId), Name = ClientName };
                DataObject messageType = new MessageType() { __PrimaryKey = Guid.Parse(MessageTypeId), Name = MessageTypeName };
                DataObject serviceBus = new Bus() { __PrimaryKey = Guid.Parse(ServiceBusId), Name = ServiceBusName, ManagerAddress = "http://localhost:12345/ServiceBus" };
                var objects = new DataObject[] { client, messageType, serviceBus };
                dataservice.UpdateObjects(ref objects);
            }
        }
    }

    public class ServiceHelperTest : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture fixture;

        public ServiceHelperTest(DatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Test for GetClient method.
        /// </summary>
        [Fact]
        public void TestGetClient()
        {
            // Arrange.
            var clientPk = Guid.Parse(DatabaseFixture.ClientId);
            var serviceBusPk = Guid.Parse(DatabaseFixture.ServiceBusId);

            foreach (var dataservice in fixture.TestDataServices)
            {
                // Act.
                var client = ServiceHelper.GetClient(clientPk, dataservice, new Mock<IStatisticsService>().Object);
                var serviceBus = ServiceHelper.GetClient(serviceBusPk, dataservice, new Mock<IStatisticsService>().Object);

                // Assert.
                Assert.NotEqual(client.GetType(), typeof(Bus));
                Assert.Equal(serviceBus.GetType(), typeof(Bus));
                Assert.True(client.Name == DatabaseFixture.ClientName && Guid.Parse(client.__PrimaryKey.ToString()) == clientPk);
                Assert.True(serviceBus.Name == DatabaseFixture.ServiceBusName && Guid.Parse(serviceBus.__PrimaryKey.ToString()) == serviceBusPk);
            }
        }

        /// <summary>
        /// Test for GetMessageType method.
        /// </summary>
        [Fact]
        public void TestGetMessageType()
        {
            // Arrange.
            var messageTypePk = Guid.Parse(DatabaseFixture.MessageTypeId);

            foreach (var dataservice in fixture.TestDataServices)
            {
                // Act.
                var messageType = ServiceHelper.GetMessageType(messageTypePk, dataservice, new Mock<IStatisticsService>().Object);

                // Assert.
                Assert.True(messageType.Name == DatabaseFixture.MessageTypeName && Guid.Parse(messageType.__PrimaryKey.ToString()) == messageTypePk);
            }
        }

        /// <summary>
        /// Test for AddSenderToMessage method.
        /// </summary>
        [Fact]
        public void TestAddSenderToMessage()
        {
            foreach (var dataservice in fixture.TestDataServices)
            {
                // Arrange.
                const string localName = "LocalTestName";
                var messageForEsb = new MessageForESB { Tags = new Dictionary<string, string> { }, ClientID = DatabaseFixture.ClientId };
                var messageForEsb2 = new MessageForESB { ClientID = "A7A7E36D-4A5F-4329-B8B1-22FC6F9C2AEE" };
                var messageForEsb3 = new MessageForESB();
                var message = new Message();
                var message2 = new Message();
                var message3 = new Message();
                var logger = new Mock<ILogger>();

                // Act.
                ServiceHelper.AddSenderToMessage(messageForEsb, message, null, dataservice, new Mock<ILogger>().Object, new Mock<IStatisticsService>().Object);
                ServiceHelper.AddSenderToMessage(messageForEsb2, message2, null, dataservice, logger.Object, new Mock<IStatisticsService>().Object);
                ServiceHelper.AddSenderToMessage(messageForEsb3, message3, new Client() { Name = localName }, dataservice, logger.Object, new Mock<IStatisticsService>().Object);

                // Assert.
                Assert.Equal(DatabaseFixture.ClientName, message.Sender);
                Assert.True(string.IsNullOrEmpty(message2.Sender));
                Assert.Equal(localName, message3.Sender);
                logger.Verify(log => log.LogUnhandledException(It.IsAny<Exception>(), It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            }
        }

        /// <summary>
        /// Test for SetMessageWithGroupValues method.
        /// </summary>
        [Fact]
        public void TestSetMessageWithGroupValues()
        {
            foreach (var dataservice in fixture.TestDataServices)
            {
                // Arrange.
                const string groupName = "LocalTestGroup";
                var msgForEsb = new MessageForESB
                {
                    Body = "Some text",
                    Priority = 1,
                    Attachment = Encoding.Unicode.GetBytes(groupName),
                    Tags = new Dictionary<string, string> { },
                    ClientID = DatabaseFixture.ClientId
                };
                var msg = new Message();
                var client = new Client();
                client.SetExistObjectPrimaryKey(Guid.Parse(DatabaseFixture.ClientId));
                var messageType = new MessageType();
                messageType.SetExistObjectPrimaryKey(Guid.Parse(DatabaseFixture.MessageTypeId));
                var subscribtion = new Subscription() { Client = client, MessageType = messageType };

                // Act.
                ServiceHelper.SetMessageWithGroupValues(msgForEsb, subscribtion, msg, groupName, dataservice, new Mock<ILogger>().Object, new Mock<IStatisticsService>().Object);

                // Assert.
                Assert.Equal(msg.Group, groupName);
                Assert.Equal(msg.Body, msgForEsb.Body);
                Assert.Equal(msg.Priority, msgForEsb.Priority);
                Assert.Equal(msg.Sender, DatabaseFixture.ClientName);
                Assert.Equal(msg.Tags, $"sendingWay:{DatabaseFixture.ClientId};senderName:{DatabaseFixture.ClientName}");
                Assert.Equal(msg.BinaryAttachment, msgForEsb.Attachment);
                Assert.Equal(msg.MessageType, messageType);
                Assert.Equal(msg.Recipient, client);
                Assert.False(string.IsNullOrEmpty(msg.Attachment));
            }
        }
    }
}
