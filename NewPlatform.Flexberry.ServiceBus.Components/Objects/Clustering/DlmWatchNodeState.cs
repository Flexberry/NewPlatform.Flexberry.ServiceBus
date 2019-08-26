namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    /// <summary>
    /// Distributed lock manager watched node state.
    /// </summary>
    public enum DlmWatchNodeState
    {
        /// <summary>
        /// Not changed.
        /// </summary>
        NotChanged,
        
        /// <summary>
        /// Changed.
        /// </summary>
        Changed,

        /// <summary>
        /// Deleted.
        /// </summary>
        Deleted,

        /// <summary>
        /// Timeout.
        /// </summary>
        TimeOut
    }
}
