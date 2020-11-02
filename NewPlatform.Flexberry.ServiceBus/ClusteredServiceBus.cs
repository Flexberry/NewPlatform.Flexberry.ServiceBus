using System.Collections.Generic;
using System.Linq;
using NewPlatform.Flexberry.ServiceBus.Clustering;
using NewPlatform.Flexberry.ServiceBus.Components;

namespace NewPlatform.Flexberry.ServiceBus
{
    public sealed class ClusteredServiceBus : BaseServiceBus<ServiceBusSettings>
    {
        protected IClusterManager ClusterManager { get; }

        protected List<ISingletonClusterComponent> SingletonComponents { get; } = new List<ISingletonClusterComponent>();

        protected List<IServiceBusComponent> MultiInstanceComponents { get; } = new List<IServiceBusComponent>();

        public ClusteredServiceBus(ILogger logger, IClusterManager clusterManager)
            : base(logger)
        {
            ClusterManager = clusterManager;
        }

        protected override void InitSettings(ServiceBusSettings settings)
        {
            SingletonComponents.Clear();
            SingletonComponents.AddRange(settings.Components.OfType<ISingletonClusterComponent>().Where(x => x.IsSingletonEnabled));
            MultiInstanceComponents.AddRange(settings.Components.Except(SingletonComponents));
        }

        protected override void StartService()
        {
            ClusterManager.InitComponents(SingletonComponents);
            ClusterManager.Start();
            MultiInstanceComponents.PrepareAndStartComponents(Logger);
        }

        protected override void StopService()
        {
            MultiInstanceComponents.StopAndDisposeComponents(Logger);
            ClusterManager.Stop();
        }
    }
}
