using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public interface IClusterManager
    {
        string CurrentInstanceKey { get; }

        string ClusterKey { get; }

        bool IsPrimary { get; }

        IDistributedLockManager DistributedLockManager { get; }

        void InitComponents(IEnumerable<ISingletonClusterComponent> components);

        void Start();

        void Stop();
    }
}
