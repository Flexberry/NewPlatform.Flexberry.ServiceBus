namespace NewPlatform.Flexberry.ServiceBus.Components.StatisticsService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.FunctionalLanguage.SQLWhere;
    using ICSSoft.STORMNET.KeyGen;
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;

    /// <summary>
    /// Component to collect statistics from Rabbit MQ
    /// </summary>
    internal class RmqStatisticsCollector : BaseServiceBusComponent, IExternalStatisticsCollector
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly IManagementClient _managementClient;
        private readonly AmqpNamingManager _namingManager;
        private readonly IStatisticsSettings _statisticsSettings;
        private IStatisticsSaveService _statisticsSaveService;
        private StatisticsInterval _interval = StatisticsInterval.OneMinute;
        private readonly string _vhostStr;
        private Vhost _vhost;
        private Dictionary<Guid, MessageStats> _previousMessageStats = new Dictionary<Guid, MessageStats>();
        private Timer _timer;

        private class SubscriptionQueueEntry
        {
            public SubscriptionQueueEntry(Subscription subscription, Queue queue)
            {
                Subscription = subscription;
                Queue = queue;
            }

            public Subscription Subscription
            {
                get;
            }

            public Queue Queue
            {
                get;
            }
        }

        private List<SubscriptionQueueEntry> GetWatchedQueues()
        {
            var result = new List<SubscriptionQueueEntry>();

            var queues = _managementClient.GetQueuesAsync().Result.ToArray();

            foreach (Subscription s in _esbSubscriptionsManager.GetSubscriptions())
            {
                var name = _namingManager.GetClientQueueName(s.Client.ID, s.MessageType.ID);
                var queue = queues.FirstOrDefault(x => x.Name.Equals(name) && x.Vhost.Equals(Vhost.Name));

                if (queue != null)
                {
                    result.Add(new SubscriptionQueueEntry(s, queue));
                }
            }

            return result;
        }

        private void ResetTimer(StatisticsInterval interval)
        {
            long ms = StatisticsIntervalToMilliseconds(interval);
            _timer.Change(ms, ms);
        }

        private void TimerCallBack(object state)
        {
            var stats = new List<StatisticsRecord>();

            try
            {
                var subscriptionsQueueEntries = GetWatchedQueues();

                foreach (var q in subscriptionsQueueEntries)
                {
                    var subPk = ((KeyGuid) (q.Subscription.__PrimaryKey)).Guid;
                    _previousMessageStats.TryGetValue(subPk, out var previousMessageStats);

                    var statisticsRecord = GetStatisticsByQueue(q, previousMessageStats);

                    if (!IsZerroStatistics(statisticsRecord))
                    {
                        stats.Add(statisticsRecord);
                    }

                    _previousMessageStats[subPk] = q.Queue.MessageStats;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error was raised while getting statistics from Rabbit MQ.", e.ToString());
            }

            try
            {
                _statisticsSaveService.Save(stats);
            }
            catch (Exception e)
            {
                _logger.LogError("A error was raised while writing statistics from Rabbit MQ to a Database.", e.ToString());
            }
        }

        private bool IsZerroStatistics(StatisticsRecord r)
        {
            return r.SentCount == 0 && r.ReceivedCount == 0 && r.UniqueErrorsCount == 0;
        }

        private StatisticsRecord GetStatisticsByQueue(SubscriptionQueueEntry e, MessageStats prevStats)
        {
            var record = new StatisticsRecord();

            var setting = _statisticsSettings.GetSetting(e.Subscription);
            if (setting == null)
            {
                setting = _statisticsSettings.CreateSetting(e.Subscription);
            }

            record.StatisticsSetting = setting;

            record.Since = DateTime.Now;
            record.To = record.Since.AddMilliseconds(StatisticsIntervalToMilliseconds(Interval));
            record.StatisticsInterval = Interval;

            var stats = e.Queue.MessageStats;

            var sumSentFunc = new Func<MessageStats, long> (x => x != null ? x.DeliverGet : 0);
            var sumPublishFunc = new Func<MessageStats, long> (x => x != null ? x.Publish : 0);

            if (stats != null)
            {
                record.SentCount = GetStatisticValue((int)sumSentFunc(stats), (int)sumSentFunc(prevStats));
                record.ReceivedCount = GetStatisticValue((int)sumPublishFunc(stats), (int)sumPublishFunc(prevStats));
                record.QueueLength = e.Queue.Messages;
            }

            return record;
        }

        private int GetStatisticValue(int newValue, int? prevValue)
        {
            if (prevValue == null)
            {
                return newValue;
            }

            if (prevValue > newValue)
            {
                return newValue;
            }

            return newValue - (int)prevValue;
        }

        private long StatisticsIntervalToMilliseconds(StatisticsInterval interval)
        {
            switch (interval)
            {
                case StatisticsInterval.OneSecond:
                    return 1000L;
                case StatisticsInterval.TenSeconds:
                    return 10 * 1000L;
                case StatisticsInterval.OneMinute:
                    return 60 * 1000L;
                case StatisticsInterval.FiveMinutes:
                    return 5 * 60 * 1000L;
                case StatisticsInterval.TenMinutes:
                    return 10 * 60 * 1000L;
                case StatisticsInterval.HalfAnHour:
                    return 30 * 60 * 1000L;
                case StatisticsInterval.Hour:
                    return 60 * 60 * 1000L;
                case StatisticsInterval.Day:
                    return 24 * 60 * 60 * 1000L;
                case StatisticsInterval.Month:
                    return 30 * 24 * 60 * 60 * 1000L;
                case StatisticsInterval.Quarter:
                    return 3 * 30 * 24 * 60 * 60 * 1000L;
                case StatisticsInterval.Year:
                    return 12 * 30 * 24 * 60 * 60 * 1000L;
            }

            return 60 * 1000L;
        }

        /// <summary>
        /// Gets Vhost RabbitMq.
        /// </summary>
        public Vhost Vhost
        {
            get
            {
                if (_vhost == null)
                {
                    _vhost = this._managementClient.CreateVirtualHostAsync(_vhostStr).Result;
                }
                return _vhost;
            }
        }

        public StatisticsInterval Interval
        {
            get => _interval;
            set
            {
                _interval = value;
                ResetTimer(_interval);
            }
        }

        /// <param name="dataService"></param>
        /// <param name="logger"></param>
        /// <param name="esbSubscriptionsManager"></param>
        /// <param name="statisticsSettings"></param>
        /// <param name="managementClient"></param>
        /// <param name="namingManager"></param>
        public RmqStatisticsCollector(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, IStatisticsSettings statisticsSettings, IManagementClient managementClient, AmqpNamingManager namingManager, IStatisticsSaveService statisticsSaveService, string vhost = "/")
        {
            this._logger = logger;
            this._esbSubscriptionsManager = esbSubscriptionsManager;
            this._managementClient = managementClient;
            this._namingManager = namingManager;
            this._statisticsSettings = statisticsSettings;
            this._statisticsSaveService = statisticsSaveService;
            this._vhostStr = vhost;

            _timer = new Timer(TimerCallBack);
        }

        public override void Prepare()
        {
            base.Prepare();
        }

        public override void Start()
        {
            base.Start();
            ResetTimer(_interval);
        }

        public override void Stop()
        {
            base.Stop();
        }
    }
}
