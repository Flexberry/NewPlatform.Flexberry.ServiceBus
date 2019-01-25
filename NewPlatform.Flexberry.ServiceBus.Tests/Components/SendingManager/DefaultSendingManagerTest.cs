namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests DefaultSendingManager component.
    /// </summary>
    public class DefaultSendingManagerTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB DefaultSendingManager component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new DefaultSendingManager(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockDataService(),
                GetMockLogger());

            RunSBComponentFullCycle(service);
        }
    }
}
