namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using Xunit;

    public class CachedDataServiceObjectRepositoryTest : BaseServiceBusTest
    {
        [Fact]
        public void TestStartStop()
        {
            var component = new CachedDataServiceObjectRepository(GetMockLogger(), GetMockDataService(), GetMockStatisticsService());

            RunSBComponentFullCycle(component);
        }
    }
}
