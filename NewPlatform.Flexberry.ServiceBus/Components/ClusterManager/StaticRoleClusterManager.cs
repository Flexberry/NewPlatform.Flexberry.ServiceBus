using NewPlatform.Flexberry.ServiceBus.Clustering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewPlatform.Flexberry.ServiceBus.Components.ClusterManager
{
    public class StaticRoleClusterManager : IClusterManager
    {
        public string CurrentInstanceKey { get; } = $"{Environment.MachineName}_{Guid.NewGuid():D}";

        public string ClusterKey => throw new NotImplementedException();

        public bool IsPrimary { get; }

        protected ILogger Logger { get; }

        protected List<ISingletonClusterComponent> SingletonClusterComponents { get; set; }

        public IDistributedLockManager DistributedLockManager => throw new NotImplementedException();

        public StaticRoleClusterManager(ILogger logger) : this(logger, false)
        { }

        public StaticRoleClusterManager(ILogger logger, bool isPrimary)
        {
            Logger = logger;
            IsPrimary = isPrimary;
        }

        public void InitComponents(IEnumerable<ISingletonClusterComponent> components)
        {
            if (IsPrimary)
            {
                if (components == null)
                {
                    throw new ArgumentNullException(nameof(components));
                }

                SingletonClusterComponents = components.ToList();
            }
        }

        public void Start()
        {
            if (IsPrimary)
            {
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Start)}",
                    $"Starting cluster instance \"{CurrentInstanceKey}\" with primary status."
                );
                SingletonClusterComponents.PrepareAndStartComponents(Logger);
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Start)}",
                    $"Singleton cluster components of cluster instance \"{CurrentInstanceKey}\" with primary status started."
                );
            }
            else
            {
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Start)}",
                    $"Starting cluster instance \"{CurrentInstanceKey}\" with not primary status."
                );
            }
        }

        public void Stop()
        {
            if (IsPrimary)
            {
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Stop)}",
                    $"Stopping singleton cluster components of cluster instance \"{CurrentInstanceKey}\" with primary status."
                );
                SingletonClusterComponents.StopAndDisposeComponents(Logger);
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Stop)}",
                    $"Cluster instance \"{CurrentInstanceKey}\" with primary status stopped."
                );
            }
            else
            {
                Logger.LogDebugMessage(
                    $"{nameof(StaticRoleClusterManager)}.{nameof(Stop)}",
                    $"Cluster instance \"{CurrentInstanceKey}\" with not primary status stopped."
                );
            }
        }
    }
}
