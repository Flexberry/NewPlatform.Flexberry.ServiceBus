namespace NewPlatform.Flexberry.ServiceBus.Tests.Components.StatisticsService
{
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    /// <summary>
    /// Test DefaultStatisticsSettings component.
    /// </summary>
    public class DefaultStatisticsSettingsTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultStatisticsSettings component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            // Arrange.
            var repo = new MockRepository(MockBehavior.Default);
            var dataServiceMock = repo.Create<IDataService>();
            var loggerMock = repo.Create<ILogger>();
            var service = new DefaultStatisticsSettings(dataServiceMock.Object, loggerMock.Object);

            // Act.
            RunSBComponentFullCycle(service);

            // Assert.
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Never);
            dataServiceMock.Verify(ds => ds.LoadObjects(It.IsAny<LoadingCustomizationStruct>()), Times.AtLeastOnce);
            repo.Verify();
        }
    }
}
