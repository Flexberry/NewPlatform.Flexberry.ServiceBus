namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    using System.Threading.Tasks;

    /// <summary>
    /// Distributed lock manager interface.
    /// </summary>
    public interface IDistributedLockManager
    {
        /// <summary>
        /// Getting lock value.
        /// </summary>
        /// <param name="key">Lock key.</param>
        /// <returns></returns>
        Task<IDlmGetLockValueResult> TryGetLockValue(string key);

        /// <summary>
        /// Adding lock.
        /// </summary>
        /// <param name="key">Lock key.</param>
        /// <param name="value">Lock value.</param>
        /// <param name="ttl">Time to live.</param>
        /// <returns></returns>
        Task<DlmAddLockResult> AddLock(string key, string value, int ttl);

        /// <summary>
        /// Updating lock ttl.
        /// </summary>
        /// <param name="key">Lock key.</param>
        /// <param name="value">Lock value.</param>
        /// <param name="ttl">Time to live.</param>
        /// <returns></returns>
        Task<DlmUpdateLockResult> UpdateLockTtl(string key, string value, int ttl);

        /// <summary>
        /// Removing lock.
        /// </summary>
        /// <param name="key">Lock key.</param>
        /// <param name="value">Lock value.</param>
        /// <returns></returns>
        Task<DlmUpdateLockResult> RemoveLock(string key, string value);

        /// <summary>
        /// Watch for lock changed.
        /// </summary>
        /// <param name="key">Lock key.</param>
        /// <param name="timeout">Watch timeout.</param>
        /// <returns></returns>
        Task<IDlmWatchResult> WatchForChange(string key, int timeout);
    }
}
