namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using ClientTools;

    /// <summary>
    /// Base implementation of component for providing access by WCF.
    /// </summary>
    internal sealed class WcfService : BaseServiceBusComponent, IWcfService
    {
        /// <summary>
        /// The subscriptions manager.
        /// </summary>
        private readonly ISubscriptionsManager _subscriptionsManager;

        /// <summary>
        /// The sending manager.
        /// </summary>
        private readonly ISendingManager _sendingManager;

        /// <summary>
        /// The receiving manager.
        /// </summary>
        private readonly IReceivingManager _receivingManager;

        private readonly ILogger _logger;

        /// <summary>
        /// The WCF service host.
        /// </summary>
        private ServiceHost _wcfServiceHost;

        /// <summary>
        /// Load WCF settings from .NET configuration file ("app.config" / "web.config").
        /// Enabled by default.
        /// </summary>
        public bool UseWcfSettingsFromConfig { get; set; } = true;

        /// <summary>
        /// Address of WCF service.
        /// </summary>
        /// <remarks>
        /// Used only when <see cref="UseWcfSettingsFromConfig"/> is <c>false</c>. Cannot be <c>null</c> in that case.
        /// </remarks>
        public Uri Address { get; set; }

        /// <summary>
        /// Binding of WCF service.
        /// </summary>
        /// <remarks>
        /// Used only when <see cref="UseWcfSettingsFromConfig"/> is <c>false</c>. Cannot be <c>null</c> in that case.
        /// </remarks>
        public Binding Binding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfService"/> class.
        /// </summary>
        /// <param name="subscriptionsManager">The subscriptions manager.</param>
        /// <param name="sendingManager">The sending manager.</param>
        /// <param name="receivingManager">The receiving manager.</param>
        /// <param name="logger">The logger.</param>
        public WcfService(ISubscriptionsManager subscriptionsManager, ISendingManager sendingManager, IReceivingManager receivingManager, ILogger logger)
        {
            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (sendingManager == null)
                throw new ArgumentNullException(nameof(sendingManager));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _subscriptionsManager = subscriptionsManager;
            _sendingManager = sendingManager;
            _receivingManager = receivingManager;
            _logger = logger;
        }

        /// <summary>
        /// Starts this component.
        /// Starts WCF service if it is enabled.
        /// </summary>
        public override void Start()
        {
            var service = new SBService(_subscriptionsManager, _sendingManager, _receivingManager);

            if (UseWcfSettingsFromConfig)
            {
                _logger.LogDebugMessage(nameof(WcfService), "Creating WCF host using configuration");

                _wcfServiceHost = new ServiceHost(service);
            }
            else
            {
                _logger.LogDebugMessage(nameof(WcfService), "Creating WCF host using specified binding and address");

                _wcfServiceHost = new ServiceHostWithoutConfiguration(service);

                _wcfServiceHost.AddServiceEndpoint(typeof(IServiceBusService), Binding, Address);
                _wcfServiceHost.AddServiceEndpoint(typeof(IServiceBusInterop), Binding, Address);
                _wcfServiceHost.AddServiceEndpoint(typeof(ICallbackSubscriber), Binding, Address);
            }

            _logger.LogDebugMessage(nameof(WcfService), "Opening WCF host");

            _wcfServiceHost.Open();

            _logger.LogDebugMessage(nameof(WcfService), "WCF host opened");

            base.Start();
        }

        /// <summary>
        /// Stops this component.
        /// Stops WCF service if it is enabled.
        /// </summary>
        public override void Stop()
        {
            if (_wcfServiceHost != null)
            {
                _logger.LogDebugMessage(nameof(WcfService), "Stopping WCF host");

                _wcfServiceHost.Abort();
                _wcfServiceHost = null;
            }

            base.Stop();
        }

        /// <summary>
        /// Special implementation of <see cref="ServiceHost"/> without data from configuration file.
        /// Used for configuring host from code for testing purposes of for flexible configuring Flexberry Service Bus.
        /// </summary>
        private class ServiceHostWithoutConfiguration : ServiceHost
        {
            /// <summary>
            /// Initializes new instance of <see cref="ServiceHostWithoutConfiguration"/> class
            /// with specified service instance.
            /// </summary>
            /// <param name="service">The WCF service instance for hosting.</param>
            public ServiceHostWithoutConfiguration(SBService service)
                : base(service)
            {
            }

            /// <summary>
            /// Overload implementation of loading configuration.
            /// Does nothing.
            /// </summary>
            protected override void ApplyConfiguration()
            {
            }
        }
    }
}
