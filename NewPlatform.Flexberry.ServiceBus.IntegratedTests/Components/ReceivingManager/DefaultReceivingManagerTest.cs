namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    using Moq;

    using NewPlatform.Flexberry.ServiceBus.Components;

    using Xunit;

    public class DefaultReceivingManagerTest : BaseServiceBusIntegratedTest
    {
        public DefaultReceivingManagerTest()
            : base("testDRM")
        {
        }

        [Fact]
        public void TestAcceptMessageOneSimpleMessage()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType };
                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                dataService.UpdateObjects(ref dataObjects);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { subscription });

                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    mockSubscriptionManager.Object,
                    GetMockSendingManager(),
                    dataService,
                    GetMockStatisticsService());
                var serviceBusMessage = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBum!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(serviceBusMessage);

                // Assert.
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .ToList();
                Assert.Equal(1, messages.Count);
                Assert.Equal(serviceBusMessage.Body, messages[0].Body);
                Assert.Equal(recipient.ID, messages[0].Recipient.ID);
                Assert.Equal(messageType.ID, messages[0].MessageType.ID);
            }
        }

        [Fact]
        public void TestAcceptMessageWithMultipleSubscriptions()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var random = new Random().Next(5, 15);
                var sender = new Client() { ID = "senderId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var dataObjects = new DataObject[] { sender, messageType };
                var recipients = new DataObject[random];
                var subscriptions = new DataObject[random];
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                for (int i = 0; i < random; i++)
                {
                    recipients[i] = new Client() { ID = $"recipient{i}Id" };
                    subscriptions[i] = new Subscription() { Client = recipients[i] as Client, MessageType = messageType };
                }

                dataService.UpdateObjects(ref recipients);
                dataService.UpdateObjects(ref dataObjects);
                dataService.UpdateObjects(ref subscriptions);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                    .Returns(subscriptions.Cast<Subscription>().Where(subscription => subscription.MessageType == messageType));

                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    mockSubscriptionManager.Object,
                    GetMockSendingManager(),
                    dataService,
                    GetMockStatisticsService());
                var serviceBusMessage = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBum!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(serviceBusMessage);

                // Assert.
                var recipientIds = recipients.Cast<Client>().OrderBy(recipient => recipient.ID).Select(recipient => recipient.ID).ToList();
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .OrderBy(message => message.Recipient.ID)
                    .ToList();
                Assert.Equal(random, messages.Count);
                for (int i = 0; i < random; i++)
                {
                    Assert.Equal(serviceBusMessage.Body, messages[i].Body);
                    Assert.Equal(recipientIds[i], messages[i].Recipient.ID);
                    Assert.Equal(messageType.ID, messages[i].MessageType.ID);
                }
            }
        }

        [Fact]
        public void TestAcceptMessageWithGroup()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType };
                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                dataService.UpdateObjects(ref dataObjects);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { subscription });

                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    mockSubscriptionManager.Object,
                    GetMockSendingManager(),
                    dataService,
                    GetMockStatisticsService());
                var messageForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBum!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };
                var newMessageForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBam!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(messageForESB, "group");
                component.AcceptMessage(newMessageForESB, "group");

                // Assert.
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .ToList();
                Assert.Equal(1, messages.Count);
                Assert.Equal("group", messages[0].Group);
                Assert.Equal(newMessageForESB.Body, messages[0].Body);
                Assert.Equal(recipient.ID, messages[0].Recipient.ID);
                Assert.Equal(messageType.ID, messages[0].MessageType.ID);
            }
        }

        [Fact]
        public void TestAcceptMessageWithGroupAndWithout()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType };
                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                dataService.UpdateObjects(ref dataObjects);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { subscription });

                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    mockSubscriptionManager.Object,
                    GetMockSendingManager(),
                    dataService,
                    GetMockStatisticsService());
                var messageForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBum!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };
                var messageWithGroupForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBam!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(messageForESB);
                component.AcceptMessage(messageWithGroupForESB, "group");

                // Assert.
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .OrderBy(message => message.Group)
                    .ToList();
                Assert.Equal(2, messages.Count);
                Assert.Null(messages[0].Group);
                Assert.Equal("group", messages[1].Group);
                Assert.Equal(messageForESB.Body, messages[0].Body);
                Assert.Equal(messageWithGroupForESB.Body, messages[1].Body);
                Assert.Equal(recipient.ID, messages[0].Recipient.ID);
                Assert.Equal(recipient.ID, messages[1].Recipient.ID);
                Assert.Equal(messageType.ID, messages[0].MessageType.ID);
                Assert.Equal(messageType.ID, messages[1].MessageType.ID);
            }
        }

        [Fact]
        public void TestAcceptMessageWithMultipleGroupAndMultipleSubscriptions()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var random = new Random().Next(5, 15);
                var sender = new Client() { ID = "senderId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var dataObjects = new DataObject[] { sender, messageType };
                var recipients = new DataObject[random];
                var subscriptions = new DataObject[random];
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                for (int i = 0; i < random; i++)
                {
                    recipients[i] = new Client() { ID = $"recipient{i}Id" };
                    subscriptions[i] = new Subscription() { Client = recipients[i] as Client, MessageType = messageType };
                }

                dataService.UpdateObjects(ref recipients);
                dataService.UpdateObjects(ref dataObjects);
                dataService.UpdateObjects(ref subscriptions);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockSubscriptionManager
                    .Setup(sm => sm.GetSubscriptionsForMsgType(It.Is<string>(id => id == messageType.ID), It.Is<string>(id => id == sender.ID)))
                    .Returns(subscriptions.Cast<Subscription>().Where(subscription => subscription.MessageType == messageType));

                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    mockSubscriptionManager.Object,
                    GetMockSendingManager(),
                    dataService,
                    GetMockStatisticsService());
                var messageForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBum!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };
                var newMessageForESB = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "BodyBam!",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(messageForESB, "group");
                component.AcceptMessage(newMessageForESB, "group");
                component.AcceptMessage(newMessageForESB, "otherGroup");

                // Assert.
                var recipientIds = recipients.Cast<Client>().OrderBy(recipient => recipient.ID).Select(recipient => recipient.ID).ToList();
                var groupedMessages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .GroupBy(message => message.Group)
                    .Select(group => new { Group = group.Key, Messages = group.OrderBy(message => message.Recipient.ID).ToList() })
                    .ToList();
                Assert.Equal(2, groupedMessages.Count);
                foreach (var messagesGroup in groupedMessages)
                {
                    Assert.Equal(random, messagesGroup.Messages.Count);
                    for (int i = 0; i < random; i++)
                    {
                        Assert.Equal("BodyBam!", messagesGroup.Messages[i].Body);
                        Assert.Equal(messagesGroup.Group, messagesGroup.Messages[i].Group);
                        Assert.Equal(recipientIds[i], messagesGroup.Messages[i].Recipient.ID);
                        Assert.Equal(messageType.ID, messagesGroup.Messages[i].MessageType.ID);
                    }
                }
            }
        }

        [Fact]
        public void TestParallelAcceptMessageWithGroup()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType, ExpiryDate = DateTime.Now.AddDays(1) };
                var sendingPermission = new SendingPermission() { Client = sender, MessageType = messageType };

                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription, sendingPermission };
                dataService.UpdateObjects(ref dataObjects);

                var statisticsService = GetMockStatisticsService();
                var component = new DefaultReceivingManager(
                    GetMockLogger(),
                    new DataServiceObjectRepository(dataService, statisticsService),
                    new DefaultSubscriptionsManager(dataService, statisticsService),
                    GetMockSendingManager(),
                    dataService,
                    statisticsService);

                // Act.
                var message = new ServiceBusMessage() { ClientID = sender.ID, MessageTypeID = messageType.ID, Body = string.Empty };
                Task.WhenAll(
                    Task.Run(() => component.AcceptMessage(message, "group")),
                    Task.Run(() => component.AcceptMessage(message, "group")),
                    Task.Run(() => component.AcceptMessage(message, "group"))).Wait();

                // Assert.
                Assert.Equal(1, ((SQLDataService)dataService).Query<Message>().Count());
            }
        }
    }
}