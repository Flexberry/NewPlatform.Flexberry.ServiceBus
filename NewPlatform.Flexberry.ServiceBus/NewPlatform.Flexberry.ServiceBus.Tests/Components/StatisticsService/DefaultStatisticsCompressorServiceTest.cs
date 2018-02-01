namespace NewPlatform.Flexberry.ServiceBus.Tests.Components.StatisticsService
{
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultStatisticsCompressorService component.
    /// </summary>
    public class DefaultStatisticsCompressorServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultStatisticsCompressorService component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var dataServiceMock = repo.Create<IDataService>();
            var statisticsTimeServiceMock = repo.Create<IStatisticsTimeService>();
            var loggerMock = repo.Create<ILogger>();
            var service = new DefaultStatisticsCompressorService(dataServiceMock.Object, statisticsTimeServiceMock.Object, loggerMock.Object, GetMockStatisticsService());

            // Act.
            RunSBComponentFullCycle(service);

            // Assert.
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            repo.Verify();
        }

    }
}
