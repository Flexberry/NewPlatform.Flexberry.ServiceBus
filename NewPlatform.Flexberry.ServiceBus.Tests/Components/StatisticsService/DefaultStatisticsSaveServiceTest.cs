namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultStatisticsSaveService component.
    /// </summary>
    public class DefaultStatisticsSaveServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultStatisticsSaveService component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var dataServiceMock = repo.Create<IDataService>();
            var loggerMock = repo.Create<ILogger>();
            var service = new DefaultStatisticsSaveService(dataServiceMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentFullCycle(service);

            // Assert.
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

        /// <summary>
        /// Doing nothing without stats.
        /// </summary>
        [Fact]
        public void TestDoingNothingWithoutStats()
        {
            var repo = new MockRepository(MockBehavior.Default);
            var dataServiceMock = repo.Create<IDataService>();
            var loggerMock = repo.Create<ILogger>();
            int calls = 0;
            DataObject[] dObjs = { };

            dataServiceMock
                .Setup(ds => ds.UpdateObjects(ref dObjs))
                .RefCallback((ref DataObject[] objects) => calls++);

            var service = new DefaultStatisticsSaveService(dataServiceMock.Object, loggerMock.Object);

            // Act && Assert.
            service.Save(new StatisticsRecord[0]);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            Assert.Equal(0, calls);
            repo.Verify();
        }

        /// <summary>
        /// Updating existed interval.
        /// </summary>
        [Fact]
        public void TestUpdatingExistedInterval()
        {
            var repo = new MockRepository(MockBehavior.Default);
            var dataServiceMock = repo.Create<IDataService>();
            var loggerMock = repo.Create<ILogger>();

            var called = false;
            var setting = new StatisticsSetting()
            {
                Subscription = new Subscription()
                {
                    MessageType = new MessageType(),
                    Client = new Client(),
                }
            };

            DataObject[] dObjs = It.IsAny<DataObject[]>();

            dataServiceMock
                .Setup(ds => ds.UpdateObjects(ref dObjs))
                .RefCallback((ref DataObject[] objects) =>
                {
                    called = true;

                    // Existed record is updated.
                    Assert.Equal(24, (objects[0] as StatisticsRecord).SentCount);
                    Assert.Equal(16, (objects[0] as StatisticsRecord).ReceivedCount);
                }).IgnoreRefMatching();
            dataServiceMock.Setup(ds => ds.LoadObjects(It.IsAny<LoadingCustomizationStruct>()))
                .Returns(new DataObject[] { new StatisticsRecord() { SentCount = 20, ReceivedCount = 10, StatisticsSetting = setting } });

            var service = new DefaultStatisticsSaveService(dataServiceMock.Object, loggerMock.Object);

            // Act && Assert.
            service.Save(new[]
            {
                // Wrong order for testing sorting records.
                new StatisticsRecord() { Since = new DateTime(2016, 01, 01, 00, 01, 00), SentCount = 7, ReceivedCount = 5, StatisticsSetting = setting }, // the second
                new StatisticsRecord() { Since = new DateTime(2016, 01, 01, 00, 00, 00), SentCount = 4, ReceivedCount = 6, StatisticsSetting = setting } // the first
            });
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            Assert.True(called);
            repo.Verify();
        }
    }
}
