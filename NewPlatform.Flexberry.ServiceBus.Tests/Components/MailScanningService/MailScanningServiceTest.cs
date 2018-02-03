namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using Flexberry.ServiceBus.Components;
    using Xunit;

    /// <summary>
    /// Tests MailScanningServiceSettings component.
    /// </summary>
    public class MailScanningServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB MailScanningServiceSettings component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var settings = new MailScanningServiceSettings { CheckMail = true };
            var service = new MailScanningService(settings, GetMockReceivingManager(), GetMockLogger());

            RunSBComponentFullCycle(service);
        }
    }
}
