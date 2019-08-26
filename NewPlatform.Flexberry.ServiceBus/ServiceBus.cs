namespace NewPlatform.Flexberry.ServiceBus
{
    using Components;

    public sealed class ServiceBus : BaseServiceBus<ServiceBusSettings>
    {
        public ServiceBus(ILogger logger)
            : base(logger)
        {
        }

        protected override void InitSettings(ServiceBusSettings settings)
        {
        }

        protected override void StartService()
        {
            Settings.Components.PrepareAndStartComponents(Logger);
        }

        protected override void StopService()
        {
            Settings.Components.StopAndDisposeComponents(Logger);
        }
    }
}
