﻿namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    using Moq;

    using NewPlatform.Flexberry.ServiceBus.Components;

    using Npgsql;
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

        /// <summary>
        /// Test for parallel reception of messages with the group.
        /// </summary>
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

        [Fact]
        public void TestAcceptMessageRestrictingQueue()
        {
            foreach (var dataService in this.DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType, RestrictQueueLength = true, MaxQueueLength = 2 };
                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                var mockLogger = new Mock<ILogger>();
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                dataService.UpdateObjects(ref dataObjects);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockObjectRepository
                    .Setup(or => or.GetSubscriptionRestrictingQueue(new List<Subscription>() { subscription }))
                    .Returns((IEnumerable<Subscription> subscriptions) => { return GetTestRescrictingSubscription(dataService, subscriptions); });
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

                var serviceBusMessage1 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body1",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                var serviceBusMessage2 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body2",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                var serviceBusMessage3 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body3",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(serviceBusMessage1);
                component.AcceptMessage(serviceBusMessage2);
                Exception ex = Assert.Throws<Exception>(() => component.AcceptMessage(serviceBusMessage3));
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .OrderBy(message => message.Group)
                    .ToList();

                // Assert.
                Assert.Equal(2, messages.Count);
                Assert.Equal($"Очередь сообщений типа {messageType.ID} для клиента {recipient.ID} переполнена, повторите отправку позже.", ex.Message);
            }
        }

        [Fact]
        public void TestAcceptMessageWithGroupRestrictingQueue()
        {
            foreach (var dataService in this.DataServices)
            {
                // Arrange.
                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId" };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription() { Client = recipient, MessageType = messageType, RestrictQueueLength = true, MaxQueueLength = 2 };
                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                var mockLogger = new Mock<ILogger>();
                var mockObjectRepository = new Mock<IObjectRepository>();
                var mockSubscriptionManager = new Mock<ISubscriptionsManager>();
                dataService.UpdateObjects(ref dataObjects);
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                mockObjectRepository
                    .Setup(or => or.GetSubscriptionRestrictingQueue(new List<Subscription>() { subscription }))
                    .Returns((IEnumerable<Subscription> subscriptions) => { return GetTestRescrictingSubscription(dataService, subscriptions); });
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

                var serviceBusMessage1 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body1",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                var serviceBusMessage2 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body2",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                var serviceBusMessage3 = new ServiceBusMessage()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = "Body3",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                };

                // Act.
                component.AcceptMessage(serviceBusMessage1, "group1");
                component.AcceptMessage(serviceBusMessage2, "group2");
                Exception ex = Assert.Throws<Exception>(() => component.AcceptMessage(serviceBusMessage3, "group3"));
                var messages = dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageEditView))
                    .Cast<Message>()
                    .OrderBy(message => message.Group)
                    .ToList();

                // Assert.
                Assert.Equal(2, messages.Count);
                Assert.Equal($"Очередь сообщений типа {messageType.ID} для клиента {recipient.ID} переполнена, повторите отправку позже.", ex.Message);
            }
        }

        private Subscription GetTestRescrictingSubscription(IDataService dataService, IEnumerable<Subscription> subscriptions)
        {
            var subscriptionMessageTypeIds = string.Join(", ", subscriptions.Select(x => $"'{x.MessageType.ID}'").Distinct());
            var subscriptionRecipientIds = string.Join(", ", subscriptions.Select(x => $"'{x.Client.ID}'").Distinct());

            var messageGroups = new List<Tuple<string, int>>();
            if (dataService is MSSQLDataService || dataService.GetType().IsSubclassOf(typeof(MSSQLDataService)))
            {
                var query = @"SELECT t.[Ид], r.[Ид], COUNT(m.primaryKey) FROM [Сообщение] AS m 
                            INNER JOIN [ТипСообщения] AS t ON m.[ТипСообщения_m0] = t.primaryKey AND t.[Ид] IN (" + subscriptionMessageTypeIds + ") " +
                            "INNER JOIN [Клиент] AS r ON m.[Получатель_m0] = r.primaryKey AND r.[Ид] IN (" + subscriptionRecipientIds + ") " +
                            "GROUP BY t.[Ид], r.[Ид] ORDER BY 3 DESC";

                using (var connection = new SqlConnection(dataService.CustomizationString))
                {
                    connection.Open();
                    var command = new SqlCommand(query, connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        messageGroups.Add(new Tuple<string, int>(reader.GetString(0), reader.GetInt32(2)));
                    }

                    reader.Close();
                    connection.Close();
                }
            }
            else if (dataService is PostgresDataService || dataService.GetType().IsSubclassOf(typeof(PostgresDataService)))
            {
                var query = "SELECT t.\"Ид\", r.\"Ид\", COUNT(m.primaryKey) FROM \"Сообщение\" AS m " +
                            "INNER JOIN \"ТипСообщения\" AS t ON m.\"ТипСообщения_m0\" = t.primaryKey AND t.\"Ид\" IN (" + subscriptionMessageTypeIds + ") " +
                            "INNER JOIN \"Клиент\" AS r ON m.\"Получатель_m0\" = r.primaryKey AND r.\"Ид\" IN (" + subscriptionRecipientIds + ") " +
                            "GROUP BY t.\"Ид\", r.\"Ид\" ORDER BY 3 DESC";

                using (var connection = new NpgsqlConnection(dataService.CustomizationString))
                {
                    var command = new NpgsqlCommand(query, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        messageGroups.Add(new Tuple<string, int>(reader.GetString(0), reader.GetInt32(2)));
                    }

                    reader.Close();
                    connection.Close();
                }
            }

            foreach (var messageGroup in messageGroups)
            {
                var subscription = subscriptions.FirstOrDefault(x => x.MessageType.ID == messageGroup.Item1 && messageGroup.Item2 >= x.MaxQueueLength);
                if (subscription != null)
                {
                    return subscription;
                }
            }

            return null;
        }
    }
}