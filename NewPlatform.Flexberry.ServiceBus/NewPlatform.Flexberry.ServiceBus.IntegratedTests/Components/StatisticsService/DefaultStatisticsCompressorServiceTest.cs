namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Linq;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business.LINQProvider;
    using Moq;
    using Xunit;

    public class DefaultStatisticsCompressorServiceTest : BaseServiceBusIntegratedTest
    {
        public DefaultStatisticsCompressorServiceTest()
        : base("TFSBDSCS")
        {
        }

        [Fact]
        public void TestPartCompression()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var statSetting = new StatisticsSetting();
                var compressionSetting = new StatisticsCompressionSetting()
                {
                    StatisticsSetting = statSetting,
                    CompressTo = StatisticsInterval.Hour,
                    StatisticsAgeCount = 1,
                    StatisticsAgeUnits = TimeUnit.Minute,
                    NextCompressTime = DateTime.Now,
                };
                var dataObjects = new DataObject[100];
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour - 1, 0, 0, 0);
                for (int i = 0; i < 100; i++)
                {
                    dataObjects[i] = new StatisticsRecord()
                    {
                        Since = date.AddMinutes(i),
                        To = date.AddMinutes(i + 1),
                        StatisticsInterval = StatisticsInterval.OneMinute,
                        SentCount = 1,
                        StatisticsSetting = statSetting,
                    };
                }

                var counter = 0;
                var mockTimeService = new Mock<IStatisticsTimeService>();
                mockTimeService
                    .Setup(ts => ts.Now)
                    .Callback(() =>
                    {
                        counter++;
                        if (counter == 5)
                            throw new Exception("As if something went wrong.");
                    }).Returns(DateTime.Now);

                dataService.UpdateObject(statSetting);
                dataService.UpdateObject(compressionSetting);
                dataService.UpdateObjects(ref dataObjects);

                var component = new DefaultStatisticsCompressorService(
                    dataService,
                    mockTimeService.Object,
                    GetMockLogger(),
                    GetMockStatisticsService())
                {
                    CompressionPeriod = 1000,
                    MaxRecordsForOneCompression = 50,
                };

                // Act.
                Act(component);

                // Assert.
                var statRecords = dataService.Query<StatisticsRecord>(StatisticsRecord.Views.CompressView)
                    .OrderBy(r => r.Since)
                    .ToArray();
                var compressedRecord = statRecords.First();
                var unCompressedRecord = statRecords.Skip(1).First();

                Assert.Equal(41, statRecords.Count());
                Assert.Equal(60, compressedRecord.SentCount);
                Assert.Equal(StatisticsInterval.Hour, compressedRecord.StatisticsInterval);
                Assert.Equal(StatisticsInterval.OneMinute, unCompressedRecord.StatisticsInterval);
            }
        }

        private void Act(DefaultStatisticsCompressorService component)
        {
            component.Prepare();
            component.Start();
            Thread.Sleep(component.CompressionPeriod * 3);
            component.Stop();
            component.AfterStop();
        }
    }
}
