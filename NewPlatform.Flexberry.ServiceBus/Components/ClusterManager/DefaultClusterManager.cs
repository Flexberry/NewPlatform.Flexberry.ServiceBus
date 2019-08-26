using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewPlatform.Flexberry.ServiceBus.Clustering;
using NewPlatform.Flexberry.ServiceBus.MultiTasking;

namespace NewPlatform.Flexberry.ServiceBus.Components.ClusterManager
{
    public class DefaultClusterManager : IClusterManager
    {
        private object _primaryChangeLockObject = new object();

        private bool _isPrimary = false;

        protected List<ISingletonClusterComponent> SingletonClusterComponents { get; set; } = new List<ISingletonClusterComponent>();

        protected ILogger Logger { get; }

        protected DefaultClusterManagerSettings Settings { get; }

        /// <summary>
        /// Timer for periodical update of data.
        /// </summary>
        private readonly AsyncPeriodicalTimer _upgradingToPrimaryTimer = new AsyncPeriodicalTimer();

        private readonly PeriodicalTimer _keepPrimaryTimer = new PeriodicalTimer();

        public string CurrentInstanceKey { get; }

        public string ClusterKey => Settings?.ClusterKey;

        public bool IsPrimary => _isPrimary;

        public IDistributedLockManager DistributedLockManager { get; }

        public DefaultClusterManager(
            IDistributedLockManager distributedLockManager,
            ILogger logger,
            DefaultClusterManagerSettings settings
        )
        {
            Settings = settings;
            DistributedLockManager = distributedLockManager;
            Logger = logger;

            CurrentInstanceKey = $"{Environment.MachineName}_{Guid.NewGuid():D}";
        }

        public void InitComponents(IEnumerable<ISingletonClusterComponent> components)
        {
            if (components == null)
                throw new ArgumentNullException(nameof(components));

            lock (_primaryChangeLockObject)
                SingletonClusterComponents = components.ToList();
        }

        protected async void KeepPrimaryInstance()
        {
            var updateTtlResult = await DistributedLockManager.UpdateLockTtl(
                ClusterKey,
                CurrentInstanceKey,
                Settings.PrimaryDlmKeyTtl
            );

            if (updateTtlResult == DlmUpdateLockResult.AnotherValue)
            {
                ResetPrimary();
            }
        }

        protected async Task WatchForChangingPrimaryTimerAction()
        {
            if (!_isPrimary)
            {
                // Attempt to upgrade to primary instance immediately.
                await TryUpgradingToPrimary();
            }

            var result = await DistributedLockManager.WatchForChange(ClusterKey, Settings.WatchForChangesTimeout);
            switch (result.State)
            {
                case DlmWatchNodeState.Deleted:
                case DlmWatchNodeState.TimeOut:
                    await TryUpgradingToPrimary();
                    break;
                case DlmWatchNodeState.Changed:
                    await CheckNeedUpgrade(result.NewValue);
                    break;
            }
        }

        protected async Task CheckNeedUpgrade(string newValue)
        {
            if (_isPrimary && newValue == CurrentInstanceKey) return;

            await TryUpgradingToPrimary();
        }

        protected async Task<bool> TryUpgradingToPrimary()
        {
            var result = await DistributedLockManager.AddLock(ClusterKey, CurrentInstanceKey, Settings.PrimaryDlmKeyTtl);
            switch (result)
            {
                case DlmAddLockResult.Success:
                    lock (_primaryChangeLockObject)
                    {
                        if (UpgradeToPrimary())
                        {
                            Logger.LogDebugMessage(
                                $"{nameof(DefaultClusterManager)}.{nameof(TryUpgradingToPrimary)}",
                                $"Upgrading cluster \"{ClusterKey}\" instance \"{CurrentInstanceKey}\" to primary started."
                            );
                            SingletonClusterComponents.PrepareAndStartComponents(Logger);
                            _keepPrimaryTimer.TryStart(KeepPrimaryInstance, Settings.KeepPrimaryInterval);
                            Logger.LogDebugMessage(
                                $"{nameof(DefaultClusterManager)}.{nameof(TryUpgradingToPrimary)}",
                                $"Upgrading cluster \"{ClusterKey}\" instance \"{CurrentInstanceKey}\" to primary completed."
                            );
                            return true;
                        }
                    }

                    break;
            }

            return false;
        }

        protected void ResetPrimary()
        {
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(ResetPrimary)}",
                $"Reseting primary status of cluster \"{ClusterKey}\" instance \"{CurrentInstanceKey}\" started."
            );
            lock (_primaryChangeLockObject)
            {
                _isPrimary = false;
                _keepPrimaryTimer.Stop();
                SingletonClusterComponents.StopAndDisposeComponents(Logger);
            }
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(ResetPrimary)}",
                $"Reseting primary status of cluster \"{ClusterKey}\" instance \"{CurrentInstanceKey}\" completed."
            );
        }

        public async void Start()
        {
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(Start)}",
                $"Starting {nameof(DefaultClusterManager)}"
            );
            await TryUpgradingToPrimary();
            _upgradingToPrimaryTimer.TryStart(WatchForChangingPrimaryTimerAction, Settings.PrimaryUpgradeCheckInterval);
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(Start)}",
                $"Started {nameof(DefaultClusterManager)}"
            );
        }

        public void Stop()
        {
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(Stop)}",
                $"Stopping {nameof(DefaultClusterManager)} - cluster \"{ClusterKey}\", instance \"{CurrentInstanceKey}\""
            );
            _upgradingToPrimaryTimer.TryStop();
            _keepPrimaryTimer.TryStop();
            if (IsPrimary)
            {
                StopPrimary();
            }

            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(Stop)}",
                $"Stopped {nameof(DefaultClusterManager)} - cluster \"{ClusterKey}\", instance \"{CurrentInstanceKey}\""
            );
        }

        private async void StopPrimary()
        {
            Logger.LogDebugMessage(
                $"{nameof(DefaultClusterManager)}.{nameof(StopPrimary)}",
                $"Stopping {nameof(DefaultClusterManager)} - cluster \"{ClusterKey}\", primary instance \"{CurrentInstanceKey}\""
            );
            var removeLockResult = await DistributedLockManager.RemoveLock(
                ClusterKey,
                CurrentInstanceKey
            );

            if (removeLockResult == DlmUpdateLockResult.Error)
            {
                Logger.LogError(
                    $"{nameof(DefaultClusterManager)}.{nameof(StopPrimary)}",
                    $"Error on removing lock while stopping cluster \"{ClusterKey}\", primary instance \"{CurrentInstanceKey}\"."
                );
            }

            ResetPrimary();
        }

        private bool UpgradeToPrimary()
        {
            if (_isPrimary)
            {
                Logger.LogError(
                    $"{nameof(DefaultClusterManager)}.{nameof(TryUpgradingToPrimary)}",
                    $"The current instance \"{CurrentInstanceKey}\" is already the primary instance in the cluster \"{ClusterKey}\"."
                );
                return false;
            }

            _isPrimary = true;
            return true;
        }
    }
}
