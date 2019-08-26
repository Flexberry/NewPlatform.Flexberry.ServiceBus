namespace NewPlatform.Flexberry.ServiceBus.Components.ClusterManager
{
    public class DefaultClusterManagerSettings
    {
        public string ClusterKey { get; set; }

        /// <summary>
        /// Keep primary interval, ms.
        /// </summary>
        public int KeepPrimaryInterval { get; set; }

        /// <summary>
        /// Upgrade to primary check interval, ms.
        /// </summary>
        public int PrimaryUpgradeCheckInterval { get; set; }

        /// <summary>
        /// Watch for changes timeout, ms.
        /// </summary>
        public int WatchForChangesTimeout { get; set; }

        /// <summary>
        /// Primary instance lock key ttl.
        /// </summary>
        public int PrimaryDlmKeyTtl { get; set; }
    }
}
