using NewPlatform.Flexberry.ServiceBus.Components;

namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public interface ISingletonClusterComponent : IServiceBusComponent
    {
        bool IsSingletonEnabled { get; }
    }
}
