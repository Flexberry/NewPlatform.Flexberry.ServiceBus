namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;
    using MultiTasking;

    /// <summary>
    /// Service Bus component for keeping and periodicaly actualizing statistics settings.
    /// </summary>
    internal class DefaultStatisticsSettings : BaseServiceBusComponent, IStatisticsSettings
    {
        /// <summary>
        /// View to load statistics settings.
        /// </summary>
        private static readonly View _view = StatisticsSetting.Views.CompressView;

        /// <summary>
        /// Cached copy of statistics settings.
        /// </summary>
        private static readonly Dictionary<Guid?, StatisticsSetting> _statSettings = new Dictionary<Guid?, StatisticsSetting>();

        /// <summary>
        /// Lock-object for a field <see cref="_statSettings"/>.
        /// </summary>
        private static readonly object _statSettingsLock = new object();

        /// <summary>
        /// Data service.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Current logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Timer for periodical update of data.
        /// </summary>
        private readonly PeriodicalTimer _periodicalTimer;

        /// <summary>
        /// Update period for statistics settings.
        /// </summary>
        public int UpdatePeriodMilliseconds { get; set; } = 60000;

        /// <summary>
        /// Key for statistics SB
        /// </summary>
        private static Guid? _subscriptionSB;

        /// <summary>
        /// Constructor for <see cref="DefaultStatisticsSettings"/>.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="logger">Logger.</param>
        public DefaultStatisticsSettings(IDataService dataService, ILogger logger)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _dataService = dataService;
            _logger = logger;

            _periodicalTimer = new PeriodicalTimer();
        }

        public Guid? GetSubscriptionSB()
        {
            if (_subscriptionSB == null)
            {
                var statSetting = _dataService.Query<StatisticsSetting>(_view).Where(x => x.Subscription == null).FirstOrDefault();

                if
                    (statSetting == null)
                {
                    statSetting = CreateSetting(null);
                }

                _subscriptionSB = new Guid(statSetting.__PrimaryKey.ToString());
            }
            return _subscriptionSB;
        }

        /// <summary>
        /// Returns all statistics settings without details.
        /// </summary>
        /// <returns>Statistics settings.</returns>
        public IEnumerable<StatisticsSetting> GetSettings()
        {
            lock (_statSettingsLock)
            {
                return new List<StatisticsSetting>(_statSettings.Values);
            }
        }

        /// <summary>
        /// Returns statistics setting for subscription.
        /// </summary>
        /// <param name="subscription">Subscription for statistics setting.</param>
        /// <returns>Statistics setting for suscription.</returns>
        public StatisticsSetting GetSetting(Subscription subscription)
        {
            lock (_statSettingsLock)
            {
                if (subscription == null)
                    return _statSettings.ContainsKey(_subscriptionSB) ? _statSettings[_subscriptionSB] : null;
                else
                {
                    var subscriptionId = new Guid(subscription.__PrimaryKey.ToString());
                    return _statSettings.ContainsKey(subscriptionId) ? _statSettings[subscriptionId] : null;
                }
            }
        }

        /// <summary>
        /// Creates new statistics setting and returns it.
        /// </summary>
        /// <param name="subscription">Subscription for statistics setting.</param>
        /// <returns>New statistics setting.</returns>
        public StatisticsSetting CreateSetting(Subscription subscription)
        {
            if (subscription == null)
            {
                var statSetting = new StatisticsSetting();
                return _statSettings[new Guid(statSetting.__PrimaryKey.ToString())] = statSetting;
            }
            else
            {
                var subscriptionId = new Guid(subscription.__PrimaryKey.ToString());
                return _statSettings[subscriptionId] = new StatisticsSetting()
                {
                    Subscription = subscription,
                };
            }
        }

        /// <summary>
        /// Prepare component.
        /// </summary>
        public override void Prepare()
        {
            base.Prepare();
            Process();
        }

        /// <summary>
        /// Start work.
        /// </summary>
        public override void Start()
        {
            base.Start();
            if (_periodicalTimer.State != PeriodicalTimer.TimerState.Working)
                _periodicalTimer.Start(Process, UpdatePeriodMilliseconds);
        }

        /// <summary>
        /// Finish work.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            if (_periodicalTimer.State == PeriodicalTimer.TimerState.Working)
                _periodicalTimer.Stop();
            Process();
        }

        /// <summary>
        /// Update statistics settings from database. This function will be called periodicaly when component is started.
        /// </summary>
        protected void Process()
        {
            try
            {
                var settingsToSave = _statSettings.Values.Where(x => x.GetStatus() == ObjectStatus.Created).Cast<DataObject>().ToArray();
                _dataService.UpdateObjects(ref settingsToSave);

                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(StatisticsSetting), _view);
                DataObject[] statSettings = _dataService.LoadObjects(lcs);

                lock (_statSettingsLock)
                {
                    _statSettings.Clear();
                    foreach (StatisticsSetting statSetting in statSettings)
                    {
                        if (statSetting.Subscription != null)
                        {
                            var subscriptionId = new Guid(statSetting.Subscription.__PrimaryKey.ToString());
                            _statSettings[subscriptionId] = statSetting;
                        }
                        else
                        {
                            _subscriptionSB = new Guid(statSetting.__PrimaryKey.ToString());
                            _statSettings[_subscriptionSB] = statSetting;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Update statistics settings from database error", exception.ToString());
            }
        }
    }
}
