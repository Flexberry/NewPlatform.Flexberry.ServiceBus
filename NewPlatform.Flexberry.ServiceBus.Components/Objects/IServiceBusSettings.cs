using System.Collections.Generic;
using NewPlatform.Flexberry.ServiceBus.Components;

namespace NewPlatform.Flexberry.ServiceBus
{
    public interface IServiceBusSettings
    {
        IEnumerable<IServiceBusComponent> Components { get; set; }
    }
}
