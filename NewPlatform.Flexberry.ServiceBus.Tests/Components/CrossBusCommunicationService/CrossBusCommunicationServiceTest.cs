namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.ServiceModel;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests CrossBusCommunicationService component.
    /// </summary>
    public class CrossBusCommunicationServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB CrossBusCommunicationService component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new CrossBusCommunicationService(GetMockSubscriptionManager(), GetMockObjectRepository(), GetMockLogger());
            service.Enabled = true;
            service.ServiceID4SB = "myid";
            service.ScanningTimeout = 10;

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Test on cloning message types.
        /// </summary>
        [Fact]
        public void TestCloningMessageTypes()
        {
            // Arrange.
            // External bus with WCF service with specified address.
            var externalSubManager = new Mock<ISubscriptionsManager>();
            externalSubManager
                .Setup(m => m.GetSubscriptions(It.Is<string>(s => s == "myid"), It.IsAny<bool>()))
                .Returns(() => new[]
                {
                    new Subscription() { MessageType = new MessageType() { ID = "First" } },
                    new Subscription() { MessageType = new MessageType() { ID = "Second" } },
                    new Subscription() { MessageType = new MessageType() { ID = "Third" } }
                });

            var wcfService = new WcfService(externalSubManager.Object, GetMockSendingManager(), GetMockReceivingManager(), GetMockLogger(), GetMockStatisticsService())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = new Uri("http://localhost:12343/SBService")
            };

            var serviceBusSettings = new ServiceBusSettings { Components = new[] { wcfService } };
            var serviceBus = new ServiceBus(serviceBusSettings, GetMockLogger());

            // Cross-bus communication service with connection to the bus.
            var repository = new Mock<IObjectRepository>();
            repository
                .Setup(r => r.GetAllServiceBuses())
                .Returns(() => new[] { new Bus() { ManagerAddress = "http://localhost:12343/SBService" } });
            repository
                .Setup(r => r.GetAllMessageTypes())
                .Returns(() => new[] { new MessageType() { ID = "Second" } });

            var crossSubManager = new Mock<ISubscriptionsManager>();

            var service = new CrossBusCommunicationService(crossSubManager.Object, repository.Object, GetMockLogger())
            {
                ScanningTimeout = 100,
                ServiceID4SB = "myid",
                CloneMessageTypesScanningCycles = 0
            };

            // Act.
            serviceBus.Start();
            RunSBComponentFullCycle(service, 1000);
            serviceBus.Stop();

            // Assert.
            crossSubManager.Verify(r => r.CreateMessageType(It.Is<NameCommentStruct>(ncs => ncs.Id == "First")), Times.Once);
            crossSubManager.Verify(r => r.CreateMessageType(It.Is<NameCommentStruct>(ncs => ncs.Id == "Second")), Times.Never);
            crossSubManager.Verify(r => r.CreateMessageType(It.Is<NameCommentStruct>(ncs => ncs.Id == "Third")), Times.Once);
        }

        /// <summary>
        /// Test on updating all subscriptions.
        /// </summary>
        [Fact]
        public void TestUpdateSubscribtions()
        {
            // Arrange.
            // External bus with WCF service with specified address.
            const string clientId = "myid";
            var externalSubManager = new Mock<ISubscriptionsManager>();
            var wcfService = new WcfService(externalSubManager.Object, GetMockSendingManager(), GetMockReceivingManager(), GetMockLogger(), GetMockStatisticsService())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = new Uri("http://localhost:12343/SBService")
            };

            var serviceBusSettings = new ServiceBusSettings { Components = new[] { wcfService } };
            var serviceBus = new ServiceBus(serviceBusSettings, GetMockLogger());

            // Cross-bus communication service with connection to the bus.
            var repository = new Mock<IObjectRepository>();
            repository
                .Setup(r => r.GetAllServiceBuses())
                .Returns(() => new[] { new Bus() { ManagerAddress = "http://localhost:12343/SBService" } });

            var service = new CrossBusCommunicationService(GetMockSubscriptionManager(), repository.Object, GetMockLogger())
            {
                ScanningTimeout = 100,
                ServiceID4SB = clientId,
                CloneMessageTypesScanningCycles = -1
            };

            // Act.
            serviceBus.Start();
            RunSBComponentFullCycle(service, 1000);
            serviceBus.Stop();

            // Assert.
            externalSubManager.Verify(sub => sub.UpdateAllSubscriptions(clientId), Times.AtLeastOnce);
            externalSubManager.Verify(sub => sub.GetSubscriptions(clientId, It.IsAny<bool>()), Times.Never);
        }
    }
}
