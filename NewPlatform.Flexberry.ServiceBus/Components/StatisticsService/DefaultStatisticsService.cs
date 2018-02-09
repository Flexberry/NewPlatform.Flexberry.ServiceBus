namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using MultiTasking;

    /// <summary>
    /// Component for recording statistics of sended and received messages.
    /// </summary>
    internal class DefaultStatisticsService : BaseServiceBusComponent, IStatisticsService
    {
        /// <summary>
        /// Current statistics settings component.
        /// </summary>
        private readonly IStatisticsSettings _statSettings;

        /// <summary>
        /// Current component for saving statistics data.
        /// </summary>
        private readonly IStatisticsSaveService _saveService;

        /// <summary>
        /// Current time component.
        /// </summary>
        private readonly IStatisticsTimeService _timeService;

        /// <summary>
        /// Current logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Lock object for multithread access to statistics data.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Statistics data.
        /// </summary>
        private readonly Dictionary<DateTime, Dictionary<Guid?, StatisticsRecord>> _statData = new Dictionary<DateTime, Dictionary<Guid?, StatisticsRecord>>();

        /// <summary>
        /// Timer for periodical update of data.
        /// </summary>
        private readonly PeriodicalTimer _periodicalTimer;

        /// <summary>
        /// Contains current state of SB.
        /// </summary>
        private Dictionary<object, Dictionary<string, string>> _currentState;

        /// <summary>
        /// Constructor for <see cref="DefaultStatisticsService"/>.
        /// </summary>
        /// <param name="statSettings">Component for getting statistics settings.</param>
        /// <param name="saveService">Component for saving statistics.</param>
        /// <param name="timeService">Component for getting current time.</param>
        /// <param name="logger">Component for logging.</param>
        public DefaultStatisticsService(IStatisticsSettings statSettings, IStatisticsSaveService saveService, IStatisticsTimeService timeService, ILogger logger)
        {
            if (statSettings == null)
                throw new ArgumentNullException(nameof(statSettings));

            if (saveService == null)
                throw new ArgumentNullException(nameof(saveService));

            if (timeService == null)
                throw new ArgumentNullException(nameof(timeService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _statSettings = statSettings;
            _saveService = saveService;
            _timeService = timeService;
            _logger = logger;

            _periodicalTimer = new PeriodicalTimer();
        }

        /// <summary>
        /// Notify statistics component that message has been received.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        public void NotifyMessageReceived(Subscription subscription)
        {
            lock (_lock)
            {
                GetCurrentStatRecord(subscription).ReceivedCount++;
                GetCurrentStatRecord(null).ReceivedCount++;
            }
        }

        /// <summary>
        /// Notify statistics component that message has been sent.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        public void NotifyMessageSent(Subscription subscription)
        {
            lock (_lock)
            {
                GetCurrentStatRecord(subscription).SentCount++;
                GetCurrentStatRecord(null).SentCount++;
            }
        }

        /// <summary>
        /// Notify statistics component that the error occurred while sending message.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        public void NotifyErrorOccurred(Subscription subscription)
        {
            lock (_lock)
            {
                GetCurrentStatRecord(subscription).ErrorsCount++;
                GetCurrentStatRecord(null).ErrorsCount++;
            }
        }

        /// <summary>
        /// Notifies that the message has been sent for calc avg time.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        /// <param name="time">Time sent message</param>
        public void NotifyAvgTimeSent(Subscription subscription, int time)
        {
            lock (_lock)
            {
                var obj = GetCurrentStatRecord(subscription);
                obj.SentAvgTime = time;

                var objSB = GetCurrentStatRecord(null);
                objSB.SentAvgTime = time;
            }
        }

        /// <summary>
        /// Notifies calc avg time execute sql.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        /// <param name="time">Time execute sql</param>
        public void NotifyAvgTimeSql(Subscription subscription, int time, string sql)
        {
            lock (_lock)
            {
                var obj = GetCurrentStatRecord(subscription);
                obj.QueryAvgTime = time;

                if (subscription != null)
                {
                    var objSB = GetCurrentStatRecord(subscription);
                    objSB.QueryAvgTime = time;
                }
            }
            _logger.LogInformation("Time execute sql", string.Format("{0} : {1}", time, sql));
        }

        /// <summary>
        /// Notify statistics component that open connection.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        /// <param name="message">Message itself.</param>
        public void NotifyIncConnectionCount(Subscription subscription, Message message)
        {
            lock (_lock)
            {
                GetCurrentStatRecord(subscription).ConnectionCount++;
                if (_currentState[message.__PrimaryKey] == null) {
                    _currentState.Add(message.__PrimaryKey, new Dictionary<string, string>());
                }
                _currentState[message.__PrimaryKey].Add("clientAddress", message.Recipient.Address);
                _currentState[message.__PrimaryKey].Add("clientName", message.Recipient.Name);
                _currentState[message.__PrimaryKey].Add("timeStart", message.SendingTime);
            }
        }

        /// <summary>
        /// Notify statistics component that close connection.
        /// </summary>
        /// <param name="subscription">Subscription for message.</param>
        /// <param name="message">Message itself.</param>
        public void NotifyDecConnectionCount(Subscription subscription, Message message)
        {
            lock (_lock)
            {
                GetCurrentStatRecord(subscription).ConnectionCount--;
                _currentState[message.__PrimaryKey] = null;
            }
        }

        /// <summary>
        /// Start work.
        /// </summary>
        public override void Start()
        {
            base.Start();
            if (_periodicalTimer.State != PeriodicalTimer.TimerState.Working)
                _periodicalTimer.Start(Process, 1000);
        }

        /// <summary>
        /// Stop work of component.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            if (_periodicalTimer.State == PeriodicalTimer.TimerState.Working)
                _periodicalTimer.Stop();
            SaveStatistics(true);
        }

        /// <summary>
        /// Save statistics to database. This function will be called periodicaly when component is started.
        /// </summary>
        protected void Process()
        {
            try
            {
                SaveStatistics(false);
            }
            catch (Exception exception)
            {
                _logger.LogError("Save statistics to database error", exception.ToString());
            }
        }

        /// <summary>
        /// Save statistics data to database.
        /// </summary>
        /// <param name="saveAll">Current time interval would not be saved if false.</param>
        private void SaveStatistics(bool saveAll)
        {
            var stats = new List<StatisticsRecord>();

            lock (_lock)
            {
                DateTime currentTimestampBucket = GetCurrentTimestampBucket();
                var newData = new Dictionary<DateTime, Dictionary<Guid?, StatisticsRecord>>();

                foreach (var bucketData in _statData)
                {
                    if (!saveAll && bucketData.Key == currentTimestampBucket)
                        newData.Add(bucketData.Key, bucketData.Value);
                    else
                        stats.AddRange(bucketData.Value.Values);
                }

                _statData.Clear();
                foreach (var stat in newData)
                    _statData[stat.Key] = stat.Value;
            }

            if (stats.Count != 0)
                _saveService.Save(stats);
        }

        /// <summary>
        /// Returns start of current time interval.
        /// </summary>
        /// <returns>Returns current timestamp bucket.</returns>
        private DateTime GetCurrentTimestampBucket()
        {
            var now = _timeService.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        /// <summary>
        /// Returns current stat record from <see cref="_statData"/>. It creates new record if necessary.
        /// </summary>
        /// <returns>Return current statistics record for provided <paramref name="subscription"/>.</returns>
        private StatisticsRecord GetCurrentStatRecord(Subscription subscription)
        {
            var subscriptionId = subscription == null ? _statSettings.GetSubscriptionSB() : new Guid(subscription.__PrimaryKey.ToString());

            DateTime currentTimestampBucket = GetCurrentTimestampBucket();

            Dictionary<Guid?, StatisticsRecord> bucketData;
            if (!_statData.TryGetValue(currentTimestampBucket, out bucketData))
            {
                _statData[currentTimestampBucket] = bucketData = new Dictionary<Guid?, StatisticsRecord>();
            }

            StatisticsRecord statRecord;
            if (!bucketData.TryGetValue(subscriptionId, out statRecord))
            {
                var statSetting = _statSettings.GetSetting(subscription) ?? _statSettings.CreateSetting(subscription);

                bucketData[subscriptionId] = statRecord = new StatisticsRecord()
                {
                    StatisticsSetting = statSetting,
                    StatisticsInterval = StatisticsInterval.OneSecond,
                    Since = currentTimestampBucket,
                    To = currentTimestampBucket.AddSeconds(1),
                };
            }

            return statRecord;
        }

        public Dictionary<object, Dictionary<string, string>> GetStats()
        {
            return this._currentState;
        }
    }
}