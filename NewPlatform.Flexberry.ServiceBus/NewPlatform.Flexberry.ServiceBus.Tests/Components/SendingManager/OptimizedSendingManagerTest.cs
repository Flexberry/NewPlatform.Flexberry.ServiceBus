namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests OptimizedSendingManager component.
    /// </summary>
    public class OptimizedSendingManagerTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB OptimizedSendingManager component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new OptimizedSendingManager(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockDataService(),
                GetMockLogger());

            RunSBComponentFullCycle(service);
        }
    }
}
