namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultStatisticsService component.
    /// </summary>
    public class DefaultStatisticsServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Data for check constructor parameters.
        /// </summary>
        public static IEnumerable<object[]> ConstructorParametersData
        {
            get
            {
                IStatisticsSettings statisticsSettings = new Mock<IStatisticsSettings>().Object;
                IStatisticsSaveService statisticsSaveService = new Mock<IStatisticsSaveService>().Object;
                IStatisticsTimeService statisticsTimeService = new Mock<IStatisticsTimeService>().Object;
                ISubscriptionsManager subscriptionsManager = new Mock<ISubscriptionsManager>().Object;
                ILogger logger = new Mock<ILogger>().Object;

                return new[]
                {
                    new object[] { null, statisticsSaveService, statisticsTimeService, subscriptionsManager, logger },
                    new object[] { statisticsSettings, null, statisticsTimeService, subscriptionsManager, logger },
                    new object[] { statisticsSettings, statisticsSaveService, null, subscriptionsManager, logger },
                    new object[] { statisticsSettings, statisticsSaveService, statisticsTimeService, null, logger },
                    new object[] { statisticsSettings, statisticsSaveService, statisticsTimeService, subscriptionsManager, null },
            };
            }
        }

        /// <summary>
        /// Service must created only with full set of dependend modules.
        /// </summary>
        /// <param name="statSettings">
        /// The stat Settings.
        /// </param>
        /// <param name="saveService">
        /// The save Service.
        /// </param>
        /// <param name="timeService">
        /// The time Service.
        /// </param>
        /// <param name="subscriptions">
        /// The subscriptions.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        [Theory]
        [MemberData(nameof(ConstructorParametersData))]
        public void TestConstructorMissingParameters(IStatisticsSettings statSettings, IStatisticsSaveService saveService, IStatisticsTimeService timeService, ISubscriptionsManager subscriptions, ILogger logger)
        {
            // Arrange.
            bool check = false;

            // Act.
            try
            {
                var service = new DefaultStatisticsService(statSettings, saveService, timeService, subscriptions, logger);
            }
            catch (ArgumentNullException)
            {
                check = true;
            }

            // Assert.
            Assert.True(check);
        }

        /// <summary>
        /// Run SB DefaultStatisticsService component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var statSettings = new Mock<IStatisticsSettings>().Object;
            var saveService = new Mock<IStatisticsSaveService>().Object;
            var timeService = new Mock<IStatisticsTimeService>().Object;
            var service = new DefaultStatisticsService(statSettings, saveService, timeService, GetMockSubscriptionManager(), GetMockLogger());

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Service must not try to save data without messages.
        /// </summary>
        [Fact]
        public void TestNotSavingWithoutMessages()
        {
            // Arrange.
            var statSettings = new Mock<IStatisticsSettings>().Object;
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timeService = new Mock<IStatisticsTimeService>().Object;
            var service = new DefaultStatisticsService(statSettings, saveServiceMock.Object, timeService, GetMockSubscriptionManager(), GetMockLogger());

            // Act.
            RunSBComponentFullCycle(service);

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Never);
        }

        /// <summary>
        /// Save data in the module shutdown.
        /// </summary>
        [Fact]
        public void TestSavingWithMessages()
        {
            // Arrange.
            var statSettingsMock = new Mock<IStatisticsSettings>();
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timeService = new Mock<IStatisticsTimeService>().Object;

            var subscriptions = new Subscription[]
            {
                new Subscription(),
                new Subscription(),
                new Subscription(),
                new Subscription(),
                new Subscription(),
            };
            var settings = subscriptions
                .Select(x => new StatisticsSetting() { Subscription = x })
                .ToDictionary(x => x.Subscription);
            var sentBySubscription = new Dictionary<Guid?, int>();

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timeService, GetMockSubscriptionManager(), GetMockLogger());

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    // TODO: from 1 to n messages in one interval => one save

                    c.NotifyMessageSent(subscriptions[0]);
                    c.NotifyMessageReceived(subscriptions[1]);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
        }

        /// <summary>
        /// Grouping Data.
        /// </summary>
        [Fact]
        public void TestGroupingData()
        {
            // Arrange.
            var statSettingsMock = new Mock<IStatisticsSettings>();
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timeService = new Mock<IStatisticsTimeService>().Object;

            var subscriptions = new Subscription[]
            {
                new Subscription(),
                new Subscription(),
                new Subscription(),
                new Subscription(),
                new Subscription(),               
            };
            var settings = subscriptions
                .Select(x => new StatisticsSetting() { Subscription = x })
                .ToDictionary(x => x.Subscription);
            var sentBySubscription = new Dictionary<Guid?, int>();

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => sub == null ? null : settings[sub]);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        foreach (var sr in srs)
                        {
                            if (sr.StatisticsSetting != null)
                            {
                                var subscriptionId = new Guid(sr.StatisticsSetting.Subscription.__PrimaryKey.ToString());
                                if (!sentBySubscription.ContainsKey(subscriptionId))
                                    sentBySubscription[subscriptionId] = sr.SentCount;
                                else
                                    sentBySubscription[subscriptionId] += sr.SentCount;
                            }
                        }
                    });
           
            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timeService, GetMockSubscriptionManager(), GetMockLogger());

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyMessageSent(subscriptions[1]);
                    c.NotifyMessageSent(subscriptions[2]);
                    c.NotifyMessageSent(subscriptions[3]);
                    c.NotifyMessageSent(subscriptions[4]);

                    c.NotifyMessageSent(subscriptions[2]);
                    c.NotifyMessageSent(subscriptions[3]);
                    c.NotifyMessageSent(subscriptions[4]);

                    c.NotifyMessageSent(subscriptions[3]);
                    c.NotifyMessageSent(subscriptions[4]);

                    c.NotifyMessageSent(subscriptions[4]);
                });

            // Assert.
            Assert.Equal(1, sentBySubscription[new Guid(subscriptions[1].__PrimaryKey.ToString())]);
            Assert.Equal(2, sentBySubscription[new Guid(subscriptions[2].__PrimaryKey.ToString())]);
            Assert.Equal(3, sentBySubscription[new Guid(subscriptions[3].__PrimaryKey.ToString())]);
            Assert.Equal(4, sentBySubscription[new Guid(subscriptions[4].__PrimaryKey.ToString())]);
            Assert.False(sentBySubscription.ContainsKey(new Guid(subscriptions[0].__PrimaryKey.ToString())));
        }

        /// <summary>
        /// Working with incomplete time intervals.
        /// </summary>
        [Fact]
        public void TestSavingByTimer()
        {
            // Arrange.
            var statSettingsMock = new Mock<IStatisticsSettings>();
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timerServiceMock = new Mock<IStatisticsTimeService>();

            var subscriptions = new Subscription[]
            {
                new Subscription(),
                new Subscription(),
            };
            var settings = subscriptions
                .Select(x => new StatisticsSetting() { Subscription = x })
                .ToDictionary(x => x.Subscription);

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => sub == null ? null : settings[sub]);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            timerServiceMock.Setup(ts => ts.Now).Returns(() => new DateTime(2000, 01, 01, 00, 00, 00, 00));

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, GetMockSubscriptionManager(), GetMockLogger());

            // Act && Assert.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    // Do not save records in the first interval.
                    c.NotifyMessageSent(subscriptions[0]);
                    Thread.Sleep(3000);
                    saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Never);

                    // Switching to the next interval.
                    // Saving records only from the first interval.
                    timerServiceMock.Setup(ts => ts.Now).Returns(() => new DateTime(2000, 01, 01, 00, 01, 00, 00));
                    saveServiceMock
                        .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                        .Callback<IEnumerable<StatisticsRecord>>(
                            srs =>
                            {
                                Assert.Equal(2, srs.Count());
                                Assert.Equal(settings[subscriptions[0]], srs.First().StatisticsSetting);
                                Assert.Equal(subscriptions[0], srs.First().StatisticsSetting.Subscription);
                            });

                    c.NotifyMessageSent(subscriptions[1]);
                    Thread.Sleep(3000);
                    saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);

                    // Saving all unsaved records after stop.
                    saveServiceMock
                        .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                        .Callback<IEnumerable<StatisticsRecord>>(
                            srs =>
                            {
                                Assert.Equal(2, srs.Count());
                                Assert.Equal(settings[subscriptions[1]], srs.First().StatisticsSetting);
                                Assert.Equal(subscriptions[1], srs.First().StatisticsSetting.Subscription);
                            });
                });
        }

        /// <summary>
        /// Saving data from multiple clients in an asynchronous mode.
        /// </summary>
        [Fact]
        public void TestSavingWithMutipleThreads()
        {
            // Arrange.
            var statSettingsMock = new Mock<IStatisticsSettings>();
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timerServiceMock = new Mock<IStatisticsTimeService>();

            var subscriptions = Enumerable
                .Repeat(0, 10)
                .Select(x => new Subscription())
                .ToArray();
            var settings = subscriptions
                .Select(x => new StatisticsSetting() { Subscription = x })
                .ToDictionary(x => x.Subscription);
            int sent = 0;

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => sub == null ? null : settings[sub]);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        sent += srs.Sum(t => t.SentCount);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, GetMockSubscriptionManager(), GetMockLogger());

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    var rand = new Random();

                    Action threadStart = () =>
                    {
                        for (int i = 0; i < 10000; i++)
                        {
                            var subscription = subscriptions[rand.Next(10)];

                            c.NotifyMessageReceived(subscription);
                            c.NotifyMessageSent(subscription);
                        }
                    };

                    var threads = Enumerable
                        .Repeat(0, 10)
                        .Select(x => new Thread(() => { threadStart(); }))
                        .ToList();

                    threads.ForEach(t => t.Start());
                    threads.ForEach(t => t.Join());
                });

            // Assert.
            Assert.Equal(200000, sent);
        }

        /// <summary>
        /// Notify errors by subscription.
        /// </summary>
        [Fact]
        public void TestNotifyErrorOccurred()
        {
            // Arrange.
            var statSettingsMock = new Mock<IStatisticsSettings>();
            var saveServiceMock = new Mock<IStatisticsSaveService>();
            var timerServiceMock = new Mock<IStatisticsTimeService>();

            var subscriptions = Enumerable
                .Repeat(0, 2)
                .Select(x => new Subscription())
                .ToArray();
            var settings = subscriptions
                .Select(x => new StatisticsSetting() { Subscription = x })
                .ToDictionary(x => x.Subscription);

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => sub == null ? null : settings[sub]);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {                        
                        Assert.Equal(3, srs.Count());
                        Assert.Equal(subscriptions[0], srs.First().StatisticsSetting?.Subscription);
                        Assert.Equal(1, srs.First().ErrorsCount);
                        Assert.Equal(subscriptions[1], srs.Last().StatisticsSetting?.Subscription);
                        Assert.Equal(2, srs.Last().ErrorsCount);
                    });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, GetMockSubscriptionManager(), GetMockLogger());

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyErrorOccurred(subscriptions[0]);
                    c.NotifyErrorOccurred(subscriptions[1]);
                    c.NotifyErrorOccurred(subscriptions[1]);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
        }

        /// <summary>
        /// Message recieved for Client and Message Type.
        /// </summary>
        [Fact]
        public void TestNotifyMessageReceived()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var statSettingsMock = repo.Create<IStatisticsSettings>();
            var saveServiceMock = repo.Create<IStatisticsSaveService>();
            var timerServiceMock = repo.Create<IStatisticsTimeService>();
            var subscriptionsManagerMock = repo.Create<ISubscriptionsManager>();
            var loggerMock = repo.Create<ILogger>();

            var client = new Client();
            var subscription = new Subscription();
            var messageType = new MessageType();

            var settings = new StatisticsSetting() { Subscription = subscription };

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => settings);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        Assert.Equal(2, srs.Count());
                        var firstSrs = srs.First();
                        Assert.Equal(subscription, firstSrs.StatisticsSetting?.Subscription);
                        Assert.Equal(1, firstSrs.ReceivedCount);
                        Assert.Equal(0, firstSrs.SentCount);
                        Assert.Equal(0, firstSrs.ErrorsCount);
                        Assert.Equal(0, firstSrs.UniqueErrorsCount);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            subscriptionsManagerMock
                .Setup(sm => sm.GetSubscriptionsForMsgType(messageType.ID, client.ID))
                .Returns(new[] { subscription });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, subscriptionsManagerMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyMessageReceived(client, messageType);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

        /// <summary>
        /// Message sent for Client and Message Type.
        /// </summary>
        [Fact]
        public void TestNotifyMessageSent()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var statSettingsMock = repo.Create<IStatisticsSettings>();
            var saveServiceMock = repo.Create<IStatisticsSaveService>();
            var timerServiceMock = repo.Create<IStatisticsTimeService>();
            var subscriptionsManagerMock = repo.Create<ISubscriptionsManager>();
            var loggerMock = repo.Create<ILogger>();

            var client = new Client();
            var subscription = new Subscription();
            var messageType = new MessageType();

            var settings = new StatisticsSetting() { Subscription = subscription };

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => settings);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        Assert.Equal(2, srs.Count());
                        var firstSrs = srs.First();
                        Assert.Equal(subscription, firstSrs.StatisticsSetting.Subscription);
                        Assert.Equal(0, firstSrs.ReceivedCount);
                        Assert.Equal(1, firstSrs.SentCount);
                        Assert.Equal(0, firstSrs.ErrorsCount);
                        Assert.Equal(0, firstSrs.UniqueErrorsCount);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            subscriptionsManagerMock
                .Setup(sm => sm.GetSubscriptionsForMsgType(messageType.ID, client.ID))
                .Returns(new[] { subscription });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, subscriptionsManagerMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyMessageSent(client, messageType);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

        /// <summary>
        /// Calc avg time sent for Client and Message Type.
        /// </summary>
        [Fact]
        public void TestNotifyAvgTimeSent()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var statSettingsMock = repo.Create<IStatisticsSettings>();
            var saveServiceMock = repo.Create<IStatisticsSaveService>();
            var timerServiceMock = repo.Create<IStatisticsTimeService>();
            var subscriptionsManagerMock = repo.Create<ISubscriptionsManager>();
            var loggerMock = repo.Create<ILogger>();

            var client = new Client();
            var subscription = new Subscription();
            var messageType = new MessageType();

            var settings = new StatisticsSetting() { Subscription = subscription };

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => settings);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        Assert.Equal(2, srs.Count());
                        var firstSrs = srs.First();
                        Assert.Equal(subscription, firstSrs.StatisticsSetting.Subscription);
                        Assert.Equal(0, firstSrs.ReceivedCount);
                        Assert.Equal(0, firstSrs.SentCount);
                        Assert.Equal(0, firstSrs.ErrorsCount);
                        Assert.Equal(0, firstSrs.UniqueErrorsCount);
                        Assert.Equal(80, firstSrs.SentAvgTime);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            subscriptionsManagerMock
                .Setup(sm => sm.GetSubscriptionsForMsgType(messageType.ID, client.ID))
                .Returns(new[] { subscription });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, subscriptionsManagerMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyAvgTimeSent(client, messageType, 100);
                    c.NotifyAvgTimeSent(client, messageType, 60);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

        /// <summary>
        /// Calc avg time sql.
        /// </summary>
        [Fact]
        public void TestNotifyAvgTimeSql()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var statSettingsMock = repo.Create<IStatisticsSettings>();
            var saveServiceMock = repo.Create<IStatisticsSaveService>();
            var timerServiceMock = repo.Create<IStatisticsTimeService>();
            var subscriptionsManagerMock = repo.Create<ISubscriptionsManager>();
            var loggerMock = repo.Create<ILogger>();

            var client = new Client();
            var subscription = new Subscription();
            var messageType = new MessageType();

            var settings = new StatisticsSetting() { Subscription = subscription };

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => settings);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        Assert.Equal(1, srs.Count());
                        var firstSrs = srs.First();
                        Assert.Equal(subscription, firstSrs.StatisticsSetting.Subscription);
                        Assert.Equal(0, firstSrs.ReceivedCount);
                        Assert.Equal(0, firstSrs.SentCount);
                        Assert.Equal(0, firstSrs.ErrorsCount);
                        Assert.Equal(0, firstSrs.UniqueErrorsCount);
                        Assert.Equal(80, firstSrs.QueryAvgTime);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            subscriptionsManagerMock
                .Setup(sm => sm.GetSubscriptionsForMsgType(messageType.ID, client.ID))
                .Returns(new[] { subscription });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, subscriptionsManagerMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyAvgTimeSql(client, messageType, 100, string.Empty);
                    c.NotifyAvgTimeSql(client, messageType, 60, string.Empty);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

        /// <summary>
        /// Open connection count.
        /// </summary>
        [Fact]
        public void TestNotifyIncConnectionCount()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var statSettingsMock = repo.Create<IStatisticsSettings>();
            var saveServiceMock = repo.Create<IStatisticsSaveService>();
            var timerServiceMock = repo.Create<IStatisticsTimeService>();
            var subscriptionsManagerMock = repo.Create<ISubscriptionsManager>();
            var loggerMock = repo.Create<ILogger>();

            var client = new Client();
            var subscription = new Subscription();
            var messageType = new MessageType();

            var settings = new StatisticsSetting() { Subscription = subscription };

            statSettingsMock
                .Setup(ds => ds.GetSetting(It.IsAny<Subscription>()))
                .Returns<Subscription>(sub => settings);

            statSettingsMock
                .Setup(ds => ds.GetSubscriptionSB())
                .Returns(Guid.NewGuid());

            saveServiceMock
                .Setup(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()))
                .Callback<IEnumerable<StatisticsRecord>>(
                    srs =>
                    {
                        Assert.Equal(1, srs.Count());
                        var firstSrs = srs.First();
                        Assert.Equal(subscription, firstSrs.StatisticsSetting.Subscription);
                        Assert.Equal(0, firstSrs.ReceivedCount);
                        Assert.Equal(0, firstSrs.SentCount);
                        Assert.Equal(0, firstSrs.ErrorsCount);
                        Assert.Equal(0, firstSrs.UniqueErrorsCount);
                        Assert.Equal(2, firstSrs.ConnectionCount);
                    });

            timerServiceMock.Setup(ts => ts.Now).Returns(() => DateTime.Now);

            subscriptionsManagerMock
                .Setup(sm => sm.GetSubscriptionsForMsgType(messageType.ID, client.ID))
                .Returns(new[] { subscription });

            var service = new DefaultStatisticsService(statSettingsMock.Object, saveServiceMock.Object, timerServiceMock.Object, subscriptionsManagerMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentAfterStart(
                service,
                c =>
                {
                    c.NotifyIncConnectionCount(client, messageType);
                    c.NotifyIncConnectionCount(client, messageType);
                    c.NotifyIncConnectionCount(client, messageType);
                    c.NotifyDecConnectionCount(client, messageType);
                });

            // Assert.
            saveServiceMock.Verify(ds => ds.Save(It.IsAny<IEnumerable<StatisticsRecord>>()), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }
    }
}
