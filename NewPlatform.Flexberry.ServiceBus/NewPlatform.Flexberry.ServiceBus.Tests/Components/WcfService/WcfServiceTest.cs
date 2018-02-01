namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using Xunit;

    /// <summary>
    /// Tests Wcf component.
    /// </summary>
    [Collection("WcfServiceTests")]
    public class WcfServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB Wcf component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new WcfService(GetMockSubscriptionManager(), GetMockSendingManager(), GetMockReceivingManager(), GetMockLogger())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = new Uri("http://localhost:12341/SBService")
            };

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Accessing WCF services is not blocked.
        /// </summary>
        [Fact]
        public void TestAccessingWcfServices()
        {
            var binding = new BasicHttpBinding();
            var address = new Uri("http://localhost:12342/SBService");
            var log = new List<bool>();

            var subscriptionManager = GetMockSubscriptionManager();
            var sendingManager = GetMockSendingManager();
            var receivingManager = GetMockReceivingManager();
            var logger = GetMockLogger();
            var service = new WcfService(subscriptionManager, sendingManager, receivingManager, logger)
            {
                UseWcfSettingsFromConfig = false,
                Binding = binding,
                Address = address
            };

            using (service)
            {
                service.Start();

                ThreadStart act = () =>
                {
                    var channelFactory = new ChannelFactory<IServiceBusService>(binding, new EndpointAddress(address));
                    var serviceBusService = channelFactory.CreateChannel();
                    bool isUp = serviceBusService.IsUp();

                    log.Add(isUp);
                };

                Thread thread1 = new Thread(act);
                Thread thread2 = new Thread(act);

                thread1.Start();
                thread2.Start();
                thread1.Join();
                thread2.Join();

                service.Stop();
            }

            Assert.Equal(2, log.Count);
            foreach (var b in log)
            {
                Assert.True(b);
            }
        }
    }
}
