namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    public class BaseSendingManagerTest : BaseServiceBusIntegratedTest
    {
        public BaseSendingManagerTest()
            : base("testBSM")
        {
        }

        [Fact]
        public void TestGetCurrentMessageCount()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var secondMessageType = new MessageType() { ID = "secondMessageTypeId" };
                var message = new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = DateTime.Now };
                var secondMessage = new Message() { Recipient = recipient, MessageType = secondMessageType, ReceivingTime = DateTime.Now };
                var dataObjects = new DataObject[] { recipient, messageType, secondMessageType, message, secondMessage };
                var component = new TestBaseSendingManager(GetMockSubscriptionManager(), GetMockStatisticsService(), dataService, GetMockLogger());
                dataService.UpdateObjects(ref dataObjects);

                // Act & Assert.
                Assert.Equal(2, component.GetCurrentMessageCount(recipient.ID));
                Assert.Equal(1, component.GetCurrentMessageCount(recipient.ID, secondMessageType.ID));
            }
        }

        [Fact]
        public void TestGetMessagesInfo()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var now = new DateTime(2000, 1, 1, 0, 0, 0);
                var random = new Random().Next(5, 15);
                var dataObjects = new DataObject[random * 3];
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var secondMessageType = new MessageType() { ID = "secondMessageTypeId" };
                dataService.UpdateObject(recipient);
                dataService.UpdateObject(messageType);
                dataService.UpdateObject(secondMessageType);
                for (int i = 0; i < random; i++)
                {
                    dataObjects[i] = new Message()
                    {
                        Priority = 1,
                        Recipient = recipient,
                        MessageType = secondMessageType,
                        ReceivingTime = now.AddYears(1),
                    };
                    dataObjects[i + random] = new Message()
                    {
                        Priority = 2,
                        Group = "group",
                        Recipient = recipient,
                        MessageType = messageType,
                        ReceivingTime = now.AddYears(2),
                    };
                    dataObjects[i + (random * 2)] = new Message()
                    {
                        Priority = 3,
                        Recipient = recipient,
                        MessageType = messageType,
                        Tags = "Color:Black;Name:Jack",
                        ReceivingTime = now.AddYears(3),
                    };
                }

                dataService.UpdateObjects(ref dataObjects);
                var component = new TestBaseSendingManager(GetMockSubscriptionManager(), GetMockStatisticsService(), dataService, GetMockLogger());

                // Act.
                var byClientId = component.GetMessagesInfo(recipient.ID);
                var byClientIdWithMax = component.GetMessagesInfo(recipient.ID, random);
                var byClientIdAndMessageTypeId = component.GetMessagesInfo(recipient.ID, secondMessageType.ID);
                var byClientIdAndMessageTypeIdWithMax = component.GetMessagesInfo(recipient.ID, secondMessageType.ID, random / 2);
                var byClientIdAndMessageTypeIdAndGroup = component.GetMessagesInfo(recipient.ID, messageType.ID, "group");
                var byClientIdAndMessageTypeIdAndGroupWithMax = component.GetMessagesInfo(recipient.ID, messageType.ID, "group", random / 2);
                var byClientIdAndMessageTypeIdAndTags = component.GetMessagesInfo(recipient.ID, messageType.ID, new[] { "Color", "Name" });
                var byClientIdAndMessageTypeIdAndTagsWithMax = component.GetMessagesInfo(recipient.ID, messageType.ID, new[] { "Color", "Name" }, random / 2);

                // Assert.
                Assert.Equal(random * 3, byClientId.ToArray().Length);
                Assert.Equal(random, byClientIdWithMax.ToArray().Length);

                Assert.Equal(random, byClientIdAndMessageTypeId.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeId.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 1));
                Assert.True(byClientIdAndMessageTypeId.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == secondMessageType.ID));
                Assert.True(byClientIdAndMessageTypeId.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(1)));

                Assert.Equal(random / 2, byClientIdAndMessageTypeIdWithMax.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeIdWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 1));
                Assert.True(byClientIdAndMessageTypeIdWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == secondMessageType.ID));
                Assert.True(byClientIdAndMessageTypeIdWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(1)));

                Assert.Equal(random, byClientIdAndMessageTypeIdAndGroup.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeIdAndGroup.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 2));
                Assert.True(byClientIdAndMessageTypeIdAndGroup.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == messageType.ID));
                Assert.True(byClientIdAndMessageTypeIdAndGroup.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(2)));

                Assert.Equal(random / 2, byClientIdAndMessageTypeIdAndGroupWithMax.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeIdAndGroupWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 2));
                Assert.True(byClientIdAndMessageTypeIdAndGroupWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == messageType.ID));
                Assert.True(byClientIdAndMessageTypeIdAndGroupWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(2)));

                Assert.Equal(random, byClientIdAndMessageTypeIdAndTags.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeIdAndTags.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 3));
                Assert.True(byClientIdAndMessageTypeIdAndTags.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == messageType.ID));
                Assert.True(byClientIdAndMessageTypeIdAndTags.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(3)));

                Assert.Equal(random / 2, byClientIdAndMessageTypeIdAndTagsWithMax.ToArray().Length);
                Assert.True(byClientIdAndMessageTypeIdAndTagsWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.Priority == 3));
                Assert.True(byClientIdAndMessageTypeIdAndTagsWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.MessageTypeID == messageType.ID));
                Assert.True(byClientIdAndMessageTypeIdAndTagsWithMax.All<ServiceBusMessageInfo>(messageInfo => messageInfo.FormingTime == now.AddYears(3)));
            }
        }

        [Fact]
        public void TestReadMessage()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipientId = "recipientId";
                var messageTypeId = "messageTypeId";
                var message = InitReadMessageTestData(recipientId, messageTypeId, dataService)["ReadMessage"];
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptions(recipientId, true))
                    .Returns(new[] { new Subscription() { MessageType = message.MessageType } });

                var component = new TestBaseSendingManager(mockSubscriptionManager.Object, GetMockStatisticsService(), dataService, GetMockLogger());

                // Act & Assert.
                Assert.Equal(message.__PrimaryKey, component.ReadMessage(message.__PrimaryKey.ToString()).__PrimaryKey);
            }
        }

        [Fact]
        public void TestReadMessageByClientAndMessageType()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipientId = "recipientId";
                var messageTypeId = "messageTypeId";
                var message = InitReadMessageTestData(recipientId, messageTypeId, dataService)["ReadMessageByClientAndMessageType"];
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptions(recipientId, true))
                    .Returns(new[] { new Subscription() { MessageType = message.MessageType } });

                var component = new TestBaseSendingManager(mockSubscriptionManager.Object, GetMockStatisticsService(), dataService, GetMockLogger());

                // Act & Assert.
                Assert.Equal(
                    message.__PrimaryKey,
                    component.ReadMessage(recipientId, messageTypeId).__PrimaryKey);
            }
        }

        [Fact]
        public void TestReadMessageByClientAndMessageTypeWithOffset()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipientId = "recipientId";
                var messageTypeId = "messageTypeId";
                var message = InitReadMessageTestData(recipientId, messageTypeId, dataService)["ReadMessageByClientAndMessageTypeWithOffset"];
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptions(recipientId, true))
                    .Returns(new[] { new Subscription() { MessageType = message.MessageType } });

                var component = new TestBaseSendingManager(mockSubscriptionManager.Object, GetMockStatisticsService(), dataService, GetMockLogger());

                // Act & Assert.
                Assert.Equal(
                    message.__PrimaryKey,
                    component.ReadMessage(recipientId, messageTypeId, 1).__PrimaryKey);
            }
        }

        [Fact]
        public void TestReadMessageByClientAndMessageTypeAndGroup()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipientId = "recipientId";
                var messageTypeId = "messageTypeId";
                var message = InitReadMessageTestData(recipientId, messageTypeId, dataService)["ReadMessageByClientAndMessageTypeAndGroup"];
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptions(recipientId, true))
                    .Returns(new[] { new Subscription() { MessageType = message.MessageType } });

                var component = new TestBaseSendingManager(mockSubscriptionManager.Object, GetMockStatisticsService(), dataService, GetMockLogger());

                // Act & Assert.
                Assert.Equal(
                    message.__PrimaryKey,
                    component.ReadMessage(recipientId, messageTypeId, "group").__PrimaryKey);
            }
        }

        [Fact]
        public void TestReadMessageByClientAndMessageTypeAndTags()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipientId = "recipientId";
                var messageTypeId = "messageTypeId";
                var message = InitReadMessageTestData(recipientId, messageTypeId, dataService)["ReadMessageByClientAndMessageTypeAndTags"];
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptions(recipientId, true))
                    .Returns(new[] { new Subscription() { MessageType = message.MessageType } });

                var component = new TestBaseSendingManager(mockSubscriptionManager.Object, GetMockStatisticsService(), dataService, GetMockLogger());

                // Act & Assert.
                Assert.Equal(
                    message.__PrimaryKey,
                    component.ReadMessage(recipientId, messageTypeId, new[] { "Color", "Name" }).__PrimaryKey);
            }
        }

        [Fact]
        public void TestCheckEventIsRaised()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipient = new Client() { ID = "recipientId" };
                var eventType = new MessageType() { ID = "eventTypeId" };
                var secondEventType = new MessageType() { ID = "secondEventTypeId" };
                var neEvent = new Message() { Recipient = recipient, MessageType = eventType, ReceivingTime = DateTime.Now };
                var dataObjects = new DataObject[] { recipient, eventType, secondEventType, neEvent };
                var component = new TestBaseSendingManager(GetMockSubscriptionManager(), GetMockStatisticsService(), dataService, GetMockLogger());
                dataService.UpdateObjects(ref dataObjects);

                // Act & Assert.
                Assert.True(component.CheckEventIsRaised(recipient.ID, eventType.ID));
                Assert.False(component.CheckEventIsRaised(recipient.ID, secondEventType.ID));
            }
        }

        [Fact]
        public void TestDeleteMessage()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var message = new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = DateTime.Now };
                var dataObjects = new DataObject[] { recipient, messageType, message };
                var component = new TestBaseSendingManager(GetMockSubscriptionManager(), GetMockStatisticsService(), dataService, GetMockLogger());
                dataService.UpdateObjects(ref dataObjects);

                // Act & Assert.
                Assert.True(component.DeleteMessage(message.__PrimaryKey.ToString()));
            }
        }

        private Dictionary<string, Message> InitReadMessageTestData(string recipientId, string messageTypeId, IDataService dataService)
        {
            var now = DateTime.Now;
            var recipient = new Client() { ID = recipientId };
            var messageType = new MessageType() { ID = messageTypeId };
            var dataObjects = new DataObject[]
            {
                recipient,
                messageType,
                new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = now },
                new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = now.AddDays(1) },
                new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = now.AddDays(2), Group = "group" },
                new Message() { Recipient = recipient, MessageType = messageType, ReceivingTime = now.AddDays(2), Tags = "Color:Black;Name:Jack" },
            };
            dataService.UpdateObjects(ref dataObjects);
            return new Dictionary<string, Message>()
            {
                { "ReadMessage", dataObjects[2] as Message },
                { "ReadMessageByClientAndMessageType", dataObjects[2] as Message },
                { "ReadMessageByClientAndMessageTypeWithOffset", dataObjects[3] as Message },
                { "ReadMessageByClientAndMessageTypeAndGroup", dataObjects[4] as Message },
                { "ReadMessageByClientAndMessageTypeAndTags", dataObjects[5] as Message },
            };
        }
    }

    internal class TestBaseSendingManager : BaseSendingManager
    {
        public TestBaseSendingManager(ISubscriptionsManager subscriptionsManager, IStatisticsService statistics, IDataService dataService, ILogger logger)
            : base(subscriptionsManager, statistics, dataService, logger)
        {
        }

        public override void QueueForSending(Message message)
        {
            throw new NotImplementedException();
        }
    }
}