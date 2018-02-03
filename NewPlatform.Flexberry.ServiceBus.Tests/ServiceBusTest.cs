namespace NewPlatform.Flexberry.ServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    /// <summary>
    /// Core service bus test.
    /// </summary>
    public partial class ServiceBusTest : BaseServiceBusTest
    {
        /// <summary>
        /// Creating empty service bus.
        /// </summary>
        [Fact]
        public void TestCreatingEmptySB()
        {
            new ServiceBus(new ServiceBusSettings(), GetMockLogger());
        }

        /// <summary>
        /// Start service bus.
        /// </summary>
        [Fact]
        public void TestStartSB()
        {
            var sb = new ServiceBus(new ServiceBusSettings(), GetMockLogger());
            StartServiceBus(sb, 5);
        }

        /// <summary>
        /// Start service bus with all components.
        /// </summary>
        [Fact]
        public void TestSBWithAllComponents()
        {
            // Arrange.
            var settings = new ServiceBusSettings
            {
                Components = new IServiceBusComponent[]
                {
                    GetWcfService(),
                    GetWebApiService(),
                    GetMailScanningService(),
                    GetCrossBusCommunicationService(),
                    GetStatisticsService(),
                    GetRerouterService(),
                    GetDefaultSendingManager(),
                    GetOptimizedSendingManager(),
                    GetCachedSubscriptionsManager()
                }
            };
            var sb = new ServiceBus(settings, GetMockLogger());

            StartServiceBus(sb, 5);
        }

        /// <summary>
        /// Test that each component receives timely call Prepare (Start, Stop), and then only one.
        /// </summary>
        [Fact]
        public void TestTimelyOnlyOneCallPrepare()
        {
            Mock<IServiceBusComponent> mockService1 = new Mock<IServiceBusComponent>();
            Mock<IServiceBusComponent> mockService2 = new Mock<IServiceBusComponent>();

            // Arrange.
            var settings = new ServiceBusSettings
            {
                Components = new[]
                {
                    mockService1.Object,
                    mockService2.Object,
                }
            };
            var sb = new ServiceBus(settings, GetMockLogger());
            sb.Start();

            mockService1.Verify(m => m.Prepare());
            mockService1.Verify(m => m.Start());

            mockService2.Verify(n => n.Prepare());
            mockService2.Verify(n => n.Start());

            sb.Stop();

            mockService1.Verify(m => m.Stop());
            mockService2.Verify(n => n.Stop());

        }

        /// <summary>
        /// Test that any errors in the call Start processed and logged.
        /// </summary>
        [Fact]
        public void TestAnyErrorsInStartProcessedAndLogged()
        {
            foreach (var ex in GetListException())
            {
                var settings = new ServiceBusSettings
                {
                    Components = new IServiceBusComponent[]
                    {
                        new ErrorServiceTest(ex, null, null)
                    }
                };

                Mock<ILogger> mock = new Mock<ILogger>();
                var sb = new ServiceBus(settings, mock.Object);

                try
                {
                    sb.Start();
                    Assert.True(false, "");
                }
                catch (Exception e)
                {
                    mock.Verify(m => m.LogUnhandledException(e, null, null, null));
                }
            }
        }

        /// <summary>
        /// Test that any errors in the call Prepare processed and logged.
        /// </summary>
        [Fact]
        public void TestAnyErrorsInPrepareProcessedAndLogged()
        {
            foreach (var ex in GetListException())
            {
                var settings = new ServiceBusSettings
                {
                    Components = new IServiceBusComponent[]
                    {
                        new ErrorServiceTest(null, null, ex)
                    }
                };

                Mock<ILogger> mock = new Mock<ILogger>();
                var sb = new ServiceBus(settings, mock.Object);

                try
                {
                    sb.Start();
                    Assert.True(false, "");
                }
                catch (Exception e)
                {
                    mock.Verify(m => m.LogUnhandledException(e, null, null, null));
                }
            }
        }

        /// <summary>
        /// Test that any errors in the call Stop processed and logged.
        /// </summary>
        [Fact]
        public void TestAnyErrorsInStopProcessedAndLogged()
        {
            foreach (var ex in GetListException())
            {
                var settings = new ServiceBusSettings
                {
                    Components = new IServiceBusComponent[]
                    {
                        new ErrorServiceTest(null, ex, null)
                    }
                };

                Mock<ILogger> mock = new Mock<ILogger>();
                var sb = new ServiceBus(settings, mock.Object);
                StartServiceBus(sb, 1);
                mock.Verify(m => m.LogUnhandledException(ex, null, null, null));
            }
        }

        /// <summary>
        /// Test that at the completion of each component receives a call Stop.
        /// </summary>
        [Fact]
        public void TestComponentReceivesCallStop()
        {
            Mock<IServiceBusComponent> mockService1 = new Mock<IServiceBusComponent>();
            Mock<IServiceBusComponent> mockService2 = new Mock<IServiceBusComponent>();

            // Arrange.
            var settings = new ServiceBusSettings
            {
                Components = new[]
                {
                    mockService1.Object,
                    mockService2.Object,
                }
            };
            var sb = new ServiceBus(settings, GetMockLogger());
            StartServiceBus(sb, 5);
            mockService1.Verify(m => m.Stop());
            mockService2.Verify(n => n.Stop());
        }

        /// <summary>
        /// Test that if an error when you call the Stop at one of the components, the other will still receive their call Stop.
        /// </summary>
        [Fact]
        public void TestAllComponentsReceiveTheirCallStop()
        {
            Mock<IServiceBusComponent> mockService1 = new Mock<IServiceBusComponent>();
            Mock<IServiceBusComponent> mockService3 = new Mock<IServiceBusComponent>();

            foreach (var ex in GetListException())
            {

                var settings = new ServiceBusSettings
                {
                    Components = new[]
                    {
                        mockService1.Object,
                        new ErrorServiceTest(null, ex, null),
                        mockService3.Object
                    }
                };

                var sb = new ServiceBus(settings, GetMockLogger());
                sb.Start();
                try
                {
                    sb.Stop();
                    Assert.True(false, "");
                }
                catch (Exception)
                {
                    mockService1.Verify(m => m.Stop());
                    mockService3.Verify(n => n.Stop());
                }
            }
        }

        /// <summary>
        /// Get WcfService.
        /// </summary>
        /// <returns>Settings WcfService <see cref="WcfService"/>.</returns>
        private WcfService GetWcfService()
        {
            return new WcfService(GetMockSubscriptionManager(), GetMockSendingManager(), GetMockReceivingManager(), GetMockLogger())
            {
                UseWcfSettingsFromConfig = false,
                Binding = new BasicHttpBinding(),
                Address = new Uri("http://localhost:1234/SBService")
            };
        }

        /// <summary>
        /// Get WebApiService.
        /// </summary>
        /// <returns>Settings WebApiService <see cref="WebApiService"/>.</returns>
        private WebApiService GetWebApiService()
        {
            return new WebApiService("http://localhost:1235/RestService/", GetMockSendingManager(), GetMockReceivingManager());
        }

        /// <summary>
        /// Get MailScanningService.
        /// </summary>
        /// <returns>Settings MailScanningService <see cref="MailScanningService"/>.</returns>
        private MailScanningService GetMailScanningService()
        {
            var settings = new MailScanningServiceSettings
            {
                CheckMail = true,
                MailScanPeriod = 10
            };
            return new MailScanningService(settings, GetMockReceivingManager(), GetMockLogger());
        }

        /// <summary>
        /// Get CrossBusCommunicationService.
        /// </summary>
        /// <returns>Settings CrossBusCommunicationService <see cref="CrossBusCommunicationService"/>.</returns>
        private CrossBusCommunicationService GetCrossBusCommunicationService()
        {
            var settings = new CrossBusCommunicationService(
                GetMockSubscriptionManager(),
                GetMockObjectRepository(),
                GetMockLogger());

            settings.Enabled = true;
            settings.ServiceID4SB = "myid";
            settings.ScanningTimeout = 10;

            return settings;
        }

        /// <summary>
        /// Get StatisticsService.
        /// </summary>
        /// <returns>Settings StatisticsService <see cref="DefaultStatisticsService"/>.</returns>
        private IStatisticsService GetStatisticsService()
        {
            return new DefaultStatisticsService(
                new Mock<IStatisticsSettings>().Object,
                new Mock<IStatisticsSaveService>().Object,
                new Mock<IStatisticsTimeService>().Object,
                GetMockSubscriptionManager(),
                GetMockLogger());
        }

        /// <summary>
        /// Get RerouterService.
        /// </summary>
        /// <returns>Settings RerouterService <see cref="RerouterService"/>.</returns>
        private RerouterService GetRerouterService()
        {
            var settings = new RerouterService(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockLogger());

            settings.Enabled = true;

            return settings;
        }

        /// <summary>
        /// Get DefaultSendingManager.
        /// </summary>
        /// <returns>Settings DefaultSendingManager <see cref="DefaultSendingManager"/>.</returns>
        private DefaultSendingManager GetDefaultSendingManager()
        {
            return new DefaultSendingManager(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockDataService(),
                GetMockLogger());
        }

        /// <summary>
        /// Get OptimizedSendingManager.
        /// </summary>
        /// <returns>Settings OptimizedSendingManager <see cref="OptimizedSendingManager"/>.</returns>
        private OptimizedSendingManager GetOptimizedSendingManager()
        {
            return new OptimizedSendingManager(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockDataService(),
                GetMockLogger());
        }

        /// <summary>
        /// Get CachedSubscriptionsManager.
        /// </summary>
        /// <returns>Settings CachedSubscriptionsManager <see cref="CachedSubscriptionsManager"/>.</returns>
        private CachedSubscriptionsManager GetCachedSubscriptionsManager()
        {
            return new CachedSubscriptionsManager(GetMockLogger(), GetMockDataService(), GetMockStatisticsService());
        }

        /// <summary>
        /// Start service bus.
        /// </summary>
        /// <param name="sb">ServiceBus <see cref="ServiceBus"/>.</param>
        /// <param name="seconds">Time in seconds.</param>
        private void StartServiceBus(ServiceBus sb, int seconds)
        {
            sb.Start();
            Thread.Sleep(seconds * 1000);
            sb.Stop();
        }

        /// <summary>
        /// All exception.
        /// </summary>
        public List<Exception> GetListException()
        {
            var ex = new List<Exception>();
            ex.Add(new ArgumentException());
            ex.Add(new SystemException());
            ex.Add(new IndexOutOfRangeException());
            ex.Add(new NullReferenceException());
            ex.Add(new AccessViolationException());
            ex.Add(new InvalidOperationException());
            ex.Add(new ArgumentNullException());
            ex.Add(new ArgumentOutOfRangeException());
            ex.Add(new ExternalException());
            ex.Add(new COMException());
            ex.Add(new SEHException());
            return ex;
        }

        /// <summary>
        /// Test component with error.
        /// </summary>
        internal class ErrorServiceTest : BaseServiceBusComponent
        {
            private Exception _exStart;
            private Exception _exStop;
            private Exception _exPrepare;

            /// <summary>
            /// Initialize.
            /// </summary>
            public ErrorServiceTest(Exception exStart, Exception exStop, Exception exPrepare)
            {
                _exStart = exStart;
                _exStop = exStop;
                _exPrepare = exPrepare;
            }

            public override void Prepare()
            {
                if (_exPrepare != null)
                    throw _exPrepare;

                base.Prepare();
            }

            public override void Start()
            {
                if (_exStart != null)
                    throw _exStart;

                base.Start();
            }

            public override void Stop()
            {
                if (_exStop != null)
                    throw _exStop;

                base.Stop();
            }
        }
    }
}
