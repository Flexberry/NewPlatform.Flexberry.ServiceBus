namespace NewPlatform.Flexberry.ServiceBus.Dlm.Etcd
{
    using System;
    using System.Threading.Tasks;

    using Clustering;
    using Components;
    using EtcdNet;

    public class EtcdDistributedLockManager : IDistributedLockManager
    {
        protected ILogger Logger { get; }

        protected EtcdClientOpitions Options { get; }

        protected EtcdClient EtcdClient { get; }

        public EtcdDistributedLockManager(ILogger logger, string[] etcdUrls, string userName = null, string password = null)
        {
            Logger = logger;
            Options = new EtcdClientOpitions
            {
                Urls = etcdUrls
            };

            if (userName != null)
                Options.Username = userName;

            if (password != null)
                Options.Password = password;

            EtcdClient = new EtcdClient(Options);
        }

        public async Task<IDlmGetLockValueResult> TryGetLockValue(string key)
        {
            try
            {
                var value = await EtcdClient.GetNodeValueAsync(key);
                return new SimpleDlmGetLockValueResult(value);
            }
            catch (EtcdCommonException.KeyNotFound)
            {
                return new SimpleDlmGetLockValueResult(DlmGetLockValueResultState.KeyNotFound);
            }
            catch (Exception e)
            {
                Logger.LogDebugMessage($"{nameof(EtcdDistributedLockManager)}.{nameof(TryGetLockValue)}", e.ToString());
                return new SimpleDlmGetLockValueResult(DlmGetLockValueResultState.Error);
            }
        }

        public async Task<DlmAddLockResult> AddLock(string key, string value, int ttl)
        {
            try
            {
                await EtcdClient.CreateNodeAsync(key, value, ttl);
                return DlmAddLockResult.Success;
            }
            catch (EtcdCommonException.NodeExist)
            {
                return DlmAddLockResult.KeyExist;
            }
            catch (Exception e)
            {
                Logger.LogDebugMessage($"{nameof(EtcdDistributedLockManager)}.{nameof(AddLock)}", e.ToString());
                return DlmAddLockResult.Error;
            }
        }

        public async Task<DlmUpdateLockResult> RemoveLock(string key, string value)
        {
            try
            {
                await EtcdClient.CompareAndDeleteNodeAsync(key, value);
                return DlmUpdateLockResult.Success;
            }
            catch (EtcdCommonException.KeyNotFound)
            {
                return DlmUpdateLockResult.AnotherValue;
            }
            catch (EtcdCommonException.TestFailed)
            {
                return DlmUpdateLockResult.AnotherValue;
            }
            catch (Exception e)
            {
                Logger.LogDebugMessage($"{nameof(EtcdDistributedLockManager)}.{nameof(RemoveLock)}", e.ToString());
                return DlmUpdateLockResult.Error;
            }
        }

        public async Task<DlmUpdateLockResult> UpdateLockTtl(string key, string value, int ttl)
        {
            try
            {
                await EtcdClient.CompareAndSwapNodeAsync(key, value, value, ttl);
                return DlmUpdateLockResult.Success;
            }
            catch (EtcdCommonException.KeyNotFound)
            {
                return DlmUpdateLockResult.AnotherValue;
            }
            catch (EtcdCommonException.TestFailed)
            {
                return DlmUpdateLockResult.AnotherValue;
            }
            catch (Exception e)
            {
                Logger.LogDebugMessage($"{nameof(EtcdDistributedLockManager)}.{nameof(UpdateLockTtl)}", e.ToString());
                return DlmUpdateLockResult.Error;
            }
        }

        public async Task<IDlmWatchResult> WatchForChange(string key, int timeout)
        {
            try
            {
                var respTask = EtcdClient.WatchNodeAsync(key, recursive: false);
                EtcdResponse resp = null;
                if (await Task.WhenAny(respTask, Task.Delay(timeout)) == respTask)
                {
                    resp = respTask.Result;
                }
                else
                {
                    return new SimpleDlmWatchResult(DlmWatchNodeState.TimeOut);
                }

                if (resp != null && resp.Node != null)
                {
                    if (resp.Node.Key.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        switch (resp.Action.ToLowerInvariant())
                        {
                            case EtcdResponse.ACTION_DELETE:
                            case EtcdResponse.ACTION_COMPARE_AND_DELETE:
                                return new SimpleDlmWatchResult(DlmWatchNodeState.Deleted);
                            case EtcdResponse.ACTION_EXPIRE:
                                return new SimpleDlmWatchResult(DlmWatchNodeState.Changed);
                            case EtcdResponse.ACTION_SET:
                            case EtcdResponse.ACTION_CREATE:
                            case EtcdResponse.ACTION_COMPARE_AND_SWAP:
                                return new SimpleDlmWatchResult(resp.Node.Value, DlmWatchNodeState.Changed);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogDebugMessage(
                    $"{nameof(EtcdDistributedLockManager)}.{nameof(WatchForChange)}",
                    ex.ToString()
                );
            }

            return new SimpleDlmWatchResult();
        }
    }
}
