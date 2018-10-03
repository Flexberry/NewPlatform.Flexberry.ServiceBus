namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;

    /// <summary>
    /// Component for saving statistics to database.
    /// </summary>
    internal class DefaultStatisticsSaveService : BaseServiceBusComponent, IStatisticsSaveService
    {
        private readonly IDataService _dataService;

        private readonly ILogger _logger;

        /// <summary>
        /// View to load statistics records.
        /// </summary>
        private static readonly View _statRecordView = new View(typeof(StatisticsRecord), View.ReadType.OnlyThatObject);

        /// <summary>
        /// Constructor for <see cref="DefaultStatisticsSaveService"/>.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="logger">Component for logging.</param>
        public DefaultStatisticsSaveService(IDataService dataService, ILogger logger)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _dataService = dataService;
            _logger = logger;
        }

        /// <summary>
        /// Save statistics records to database.
        /// </summary>
        /// <param name="stats">Statistics records.</param>
        public void Save(IEnumerable<StatisticsRecord> stats)
        {
            DataObject[] objects = PrepareStatsForSaving(stats);
            if (objects.Length != 0)
            {
                _dataService.UpdateObjects(ref objects, true);
            }
        }

        /// <summary>
        /// Normalize statistics records for saving.
        /// </summary>
        /// <param name="stats">Raw statistics records.</param>
        /// <returns>Normalized statistics records.</returns>
        protected DataObject[] PrepareStatsForSaving(IEnumerable<StatisticsRecord> stats)
        {
            var allRecords = new List<DataObject>();

            if (stats.Count() > 0)
            {
#if DEBUG
                if (stats.Any(i => i.StatisticsInterval != stats.First().StatisticsInterval))
                    throw new ArgumentException("Stat records for saving must be with the same interval type.");
#endif

                var ldef = ExternalLangDef.LanguageDef;
                foreach (var statGroup in stats.GroupBy(x => x.StatisticsSetting.__PrimaryKey))
                {
                    var statRecords = statGroup.OrderBy(x => x.Since).ToList();
                    var firstRecord = statRecords.First();

                    if (firstRecord.StatisticsSetting.Subscription != null)
                    {
                        if (firstRecord.StatisticsSetting.Subscription.MessageType != null
                                && firstRecord.StatisticsSetting.Subscription.Client != null)
                        {
                            var queueLength = _dataService.Query<Message>(Message.Views.MessageEditView)
                                .Count(x => x.MessageType.__PrimaryKey == firstRecord.StatisticsSetting.Subscription.MessageType.__PrimaryKey
                                && x.Recipient.__PrimaryKey == firstRecord.StatisticsSetting.Subscription.Client.__PrimaryKey);

                            var errorLenth = _dataService.Query<Message>(Message.Views.MessageEditView)
                                 .Count(x => x.MessageType.__PrimaryKey == firstRecord.StatisticsSetting.Subscription.MessageType.__PrimaryKey
                                 && x.Recipient.__PrimaryKey == firstRecord.StatisticsSetting.Subscription.Client.__PrimaryKey
                                 && x.ErrorCount > 0);

                            foreach (var rec in statRecords)
                            {
                                rec.UniqueErrorsCount = errorLenth;
                                rec.QueueLength = queueLength;
                            }

                            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(StatisticsRecord), _statRecordView);
                            lcs.ReturnTop = 1;
                            lcs.LimitFunction = ldef.GetFunction(
                                ldef.funcAND,
                                ldef.GetFunction(ldef.funcEQ, new VariableDef(ldef.DateTimeType, Information.ExtractPropertyPath<StatisticsRecord>(x => x.Since)), firstRecord.Since),
                                ldef.GetFunction(ldef.funcEQ, new VariableDef(ldef.GuidType, Information.ExtractPropertyPath<StatisticsRecord>(x => x.StatisticsSetting)), firstRecord.StatisticsSetting.__PrimaryKey));

                            DataObject[] existedStatRecords = null;
                            try
                            {
                                existedStatRecords = _dataService.LoadObjects(lcs);
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError("Load statistics error. Service Bus may write double statistics record.", exception.ToString());
                            }

                            if (existedStatRecords != null && existedStatRecords.Length > 0)
                            {
                                var esr = (StatisticsRecord)existedStatRecords[0];

                                var oldSentCount = esr.SentCount;
                                var oldReceivedCount = esr.ReceivedCount;

                                // TODO: other properties
                                esr.SentCount += firstRecord.SentCount;
                                esr.ReceivedCount += firstRecord.ReceivedCount;

                                statRecords[0] = esr;

                                var logMessage = $"Existed stat record from {esr.Since} to {esr.To} (statistics setting: '{firstRecord.StatisticsSetting.__PrimaryKey}') has been updated. "
                                    + $"Old values: sent: {oldSentCount}, received: {oldReceivedCount}. "
                                    + $"New values: sent: {esr.SentCount}, received: {esr.ReceivedCount}.";
                                _logger.LogInformation($"Updating existed stat record ({esr.Since})", logMessage);
                            }
                        }
                    }
                    else
                    {
                        statRecords = statRecords.Where(x => x.SentCount > 0 || x.ReceivedCount > 0 || x.ErrorsCount > 0 || x.UniqueErrorsCount > 0 || x.QueueLength > 0).ToList();
                        if (statRecords.Count > 0)
                        {
                            var queueLength = _dataService.Query<Message>(Message.Views.MessageEditView).Count();
                            var errorLenth = _dataService.Query<Message>(Message.Views.MessageEditView).Count(x => x.ErrorCount > 0);

                            foreach (var rec in statRecords)
                            {
                                rec.UniqueErrorsCount = errorLenth;
                                rec.QueueLength = queueLength;
                            }
                        }
                    }

                    allRecords.AddRange(statRecords);
                }
            }

            return allRecords.ToArray();
        }
    }
}