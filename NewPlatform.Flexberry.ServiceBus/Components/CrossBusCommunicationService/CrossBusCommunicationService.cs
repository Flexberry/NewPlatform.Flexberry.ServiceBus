namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// Base implementation of component for communication with other insaences Flexberry Service Bus.
    /// </summary>
    internal sealed class CrossBusCommunicationService : BaseServiceBusComponent, ICrossBusCommunicationService
    {
        /// <summary>
        /// The subscriptions manager.
        /// </summary>
        private readonly ISubscriptionsManager _subscriptionsManager;

        /// <summary>
        /// The objects repository.
        /// </summary>
        private readonly IObjectRepository _repository;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The thread for asynchronous communication with other instances Flexberry Service Bus.
        /// </summary>
        private readonly Thread _scanThread;

        /// <summary>
        /// The event for waiting next synchronization.
        /// </summary>
        private readonly ManualResetEvent _scanWaitEvent = new ManualResetEvent(false);

        /// <summary>
        /// The event for waiting <see cref="_scanThread"/> to stop.
        /// </summary>
        private readonly ManualResetEvent _scanStopEvent = new ManualResetEvent(false);

        /// <summary>
        /// Cross-bus communication service is enabled.
        /// Enabled by default.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Client identifier for remote instance Flexberry Service Bus.
        /// </summary>
        public string ServiceID4SB { get; set; }

        /// <summary>
        /// Timeout in milliseconds for communication with remote instance Flexberry Service Bus.
        /// 1100 by default.
        /// </summary>
        public int ScanningTimeout { get; set; } = 1100;

        /// <summary>
        /// Count of cycles for cloning message types.
        /// </summary>
        /// <remarks>
        /// This setting is used for reducing merging message types with remote instance Flexberry Service Bus.
        /// Possible values:
        /// <list type="bullet">
        ///     <item><description>-1 : cloning is disabled;</description></item>
        ///     <item><description> 0 : clone only once;</description></item>
        ///     <item><description> N : clone each N-th cycle.</description></item>
        /// </list>
        /// </remarks>
        public int CloneMessageTypesScanningCycles { get; set; } = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossBusCommunicationService"/> class with specified settings.
        /// </summary>
        /// <param name="subscriptionsManager">The subscriptions manager.</param>
        /// <param name="repository">The objects repository.</param>
        /// <param name="logger">The logger.</param>
        public CrossBusCommunicationService(
            ISubscriptionsManager subscriptionsManager,
            IObjectRepository repository,
            ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            _repository = repository;
            _logger = logger;
            _subscriptionsManager = subscriptionsManager;

            _scanThread = new Thread(Scan)
            {
                Name = $"{nameof(CrossBusCommunicationService)}.{nameof(Scan)}"
            };
        }

        /// <summary>
        /// Starts this component.
        /// Starts scanning thread.
        /// </summary>
        public override void Start()
        {
            if (Enabled)
            {
                _logger.LogDebugMessage(nameof(CrossBusCommunicationService), "Component is enabled. Starting.");

                _scanThread.Start();
            }
            else
            {
                _logger.LogDebugMessage(nameof(CrossBusCommunicationService), "Component is not enabled.");
            }

            base.Start();
        }

        /// <summary>
        /// Stops this component.
        /// Stops scanning thread.
        /// </summary>
        public override void Stop()
        {
            _logger.LogDebugMessage(nameof(CrossBusCommunicationService), "Stopping component.");

            if (Enabled)
            {
                _scanWaitEvent.Set();
                _scanStopEvent.WaitOne();
            }

            base.Stop();
        }

        /// <summary>
        /// Disposes kernel objects.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanWaitEvent?.Dispose();
                _scanStopEvent?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Scanning thread method.
        /// </summary>
        private void Scan()
        {
            try
            {
                int cycleIndex = 0;

                // Clone message types at the first cycle unless it's disabled.
                bool cloneMessageTypes = CloneMessageTypesScanningCycles != -1;

                while (true)
                {
                    Subscribe(cloneMessageTypes);

                    // Do we need to clone message types next time?
                    if (CloneMessageTypesScanningCycles > 0)
                    {
                        cycleIndex++;
                        cloneMessageTypes = cycleIndex % CloneMessageTypesScanningCycles == 0;
                    }
                    else
                    {
                        cloneMessageTypes = false;
                    }

                    if (_scanWaitEvent.WaitOne(ScanningTimeout))
                        return;
                }
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e, title: nameof(CrossBusCommunicationService));
            }
            finally
            {
                // Notify main thread that scanning thread has stopped.
                _scanStopEvent.Set();
            }
        }

        /// <summary>
        /// Subscribes for messages from other instance Flexberry Service Bus.
        /// Clones message types and updates subscriptions in all remote instance Flexberry Service Bus.
        /// </summary>
        /// <param name="cloneMessageTypes">Clone all message types from remote instance Flexberry Service Bus.</param>
        private void Subscribe(bool cloneMessageTypes)
        {
            foreach (Bus serviceBus in _repository.GetAllServiceBuses())
            {
                try
                {
                    _logger.LogDebugMessage(nameof(CrossBusCommunicationService), $"Working with remote SB '{serviceBus.Name ?? "<noname>"}' ({serviceBus.ManagerAddress}).");

                    var client = new ChannelFactory<IServiceBusInterop>(new BasicHttpBinding()).CreateChannel(new EndpointAddress(serviceBus.ManagerAddress));
                    if (client == null)
                        throw new InvalidOperationException($"Unable to create WCF client to the SB ({serviceBus.ManagerAddress}).");

                    if (cloneMessageTypes)
                    {
                        NameCommentStruct[] msgTypes = client.GetMsgTypesFromBus(ServiceID4SB);

                        _logger.LogDebugMessage(nameof(CrossBusCommunicationService), $"Loaded {msgTypes.Length} message types from remote SB '{serviceBus.Name ?? "<noname>"}'.");

                        foreach (NameCommentStruct type in msgTypes.Where(type => _repository.GetAllMessageTypes().All(t => t.ID != type.Id)))
                        {
                            _logger.LogDebugMessage(nameof(CrossBusCommunicationService), $"Cloning message type {type.Id} ({type.Name}).");

                            _subscriptionsManager.CreateMessageType(type);
                        }
                    }
                    else
                    {
                        client.UpdateClientSubscribesForMsgs(ServiceID4SB);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogUnhandledException(e, title: nameof(CrossBusCommunicationService));
                }
            }
        }
    }
}
