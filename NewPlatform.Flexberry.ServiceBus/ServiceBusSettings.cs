namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Collections.Generic;
    using Components;

    public class ServiceBusSettings : IServiceBusSettings
    {
        public IEnumerable<IServiceBusComponent> Components { get; set; } = new IServiceBusComponent[0];
    }
}