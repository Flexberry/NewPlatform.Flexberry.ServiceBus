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
    /// Класс, реализующий функциональность сжатия данных статистики.
    /// </summary>
    internal class DefaultStatisticsCompressorService : BaseServiceBusComponent, IStatisticsCompressorService
    {
        /// <summary>
        /// The frequency at which the statistics compression task runs in milliseconds.
        /// </summary>
        public int CompressionPeriod { get; set; } = 1000 * 60;

        /// <summary>
        /// Maximum records count for compression, read from DB for one query.
        /// If 0 without limit.
        /// </summary>
        public int MaxRecordsForOneCompression { get; set; } = 1000;

        private readonly IDataService _dataService;

        private readonly ILogger _logger;

        /// <summary>
        /// Current time component.
        /// </summary>
        private readonly IStatisticsTimeService _timeService;

        /// <summary>
        /// Timer for periodical update of data.
        /// </summary>
        private readonly PeriodicalTimer _periodicalTimer;

        private readonly IStatisticsService _statisticsService;

        /// <summary>
        /// Класс, содержащий дополнительную информацию по интервалу сжатия статистики.
        /// </summary>
        private class PreparedInterval
        {
            /// <summary>
            /// Настройка интервала.
            /// </summary>
            public StatisticsCompressionSetting Setting { get; set; }

            /// <summary>
            /// Сжиматься должны интервалы старше этого времени.
            /// </summary>
            public DateTime TimeLimit { get; set; }

            /// <summary>
            /// Начало интервала сжатия статистики, выровненное по размеру интервала (StatIntervalType).
            /// </summary>
            public DateTime IntervalStart { get; set; }

            /// <summary>
            /// Конец интервала сжатия статистики, выровненный по размеру интервала (StatIntervalType).
            /// </summary>
            public DateTime IntervalEnd { get; set; }
        }

        /// <summary>
        /// Constructor for <see cref="DefaultStatisticsCompressorService"/>.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="timeService">Component for getting current time.</param>
        /// <param name="logger">Component for logging.</param>
        public DefaultStatisticsCompressorService(IDataService dataService, IStatisticsTimeService timeService, ILogger logger, IStatisticsService statisticsService)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (timeService == null)
                throw new ArgumentNullException(nameof(timeService));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            _dataService = dataService;
            _logger = logger;
            _timeService = timeService;
            _periodicalTimer = new PeriodicalTimer();
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Start work.
        /// </summary>
        public override void Start()
        {
            base.Start();
            _periodicalTimer.TryStart(CompressStatistics, CompressionPeriod);
        }

        /// <summary>
        /// Stop work of component.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            _periodicalTimer.TryStop();
        }

        /// <summary>
        /// Сжать статистику.
        /// </summary>
        public void CompressStatistics()
        {
            try
            {
                // Вычитка всех конфигураций статистики.
                var statSettings = _dataService
                    .Query<StatisticsSetting>(StatisticsSetting.Views.CompressView)
                    .ToList();

                foreach (var statSetting in statSettings)
                {
                    // Выбор конфигураций сжатия, чья очередь выполнения подошла.
                    var now = _timeService.Now;
                    var compressionSettings = statSetting.StatisticsCompressionSetting
                        .Cast<StatisticsCompressionSetting>()
                        .Where(x => x.NextCompressTime <= now)
                        .OrderByDescending(x => x.CompressTo)
                        .ToList();

                    if (!compressionSettings.Any())
                        continue;

                    // Сжатие интервалов.
                    foreach (var setting in compressionSettings)
                    {
                        var recordsToCompress = LoadRecordsForCompression(setting);
                        while (recordsToCompress
                            .GroupBy(r => GetStartOfInterval(r.Since, setting.CompressTo))
                            .Any(g => g.Count() > 1 || (g.Count() == 1 && g.First().StatisticsInterval != setting.CompressTo)))
                        {
                            var compressedRecords = CompressRecords(recordsToCompress, setting);

                            // Отметка старых записей на удаление.
                            foreach (var oldRecord in recordsToCompress)
                                oldRecord.SetStatus(ObjectStatus.Deleted);

                            // Обновление записей в таблице.
                            var recordsToUpdate = recordsToCompress
                                .Concat(compressedRecords)
                                .Cast<DataObject>()
                                .ToArray();

                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            _dataService.UpdateObjects(ref recordsToUpdate, true);

                            stopwatch.Stop();
                            long time = stopwatch.ElapsedMilliseconds;
                            _statisticsService.NotifyAvgTimeSql(setting.StatisticsSetting.Subscription, (int)time, "DefaultStatisticsCompressorService.CompressStatistics() update StatRecords.");

                            recordsToCompress = LoadRecordsForCompression(setting);
                        }

                        // Настройку сжатия тоже следует обновить.
                        setting.NextCompressTime = AddTimeUnitsToDate(_timeService.Now, setting.CompressFrequencyUnits, setting.CompressFrequencyCount);
                        setting.LastCompressTime = _timeService.Now;
                        _dataService.UpdateObject(setting);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Compress statistics error", exception.ToString());
            }
        }

        /// <summary>
        /// Загружает подлежащие сжатию записи.
        /// </summary>
        /// <param name="compressSetting">Настройка сжатия статистики.</param>
        /// <returns>Список записей статистики, загруженный из БД.</returns>
        private List<StatisticsRecord> LoadRecordsForCompression(StatisticsCompressionSetting compressSetting)
        {
            // Сжиматься должны интервалы старше этого времени.
            var timeLimit = AddTimeUnitsToDate(_timeService.Now, compressSetting.StatisticsAgeUnits, -compressSetting.StatisticsAgeCount);

            // После сжатия должно получиться целое число новых интервалов, поэтому вычисляем точный конец сжимаемого промежутка времени.
            var timeEnd = GetStartOfInterval(timeLimit, compressSetting.CompressTo);

            // Формирование основного условия для выборки.
            // Выбираются только записи, интервал которых меньше целевого.
            var recordsQuery = _dataService.Query<StatisticsRecord>(StatisticsRecord.Views.CompressView);
            var oneSecond = StatisticsInterval.OneSecond;
            var tenSeconds = StatisticsInterval.TenSeconds;
            var oneMinute = StatisticsInterval.OneMinute;
            var fiveMinutes = StatisticsInterval.FiveMinutes;
            var tenMinutes = StatisticsInterval.TenMinutes;
            var hour = StatisticsInterval.Hour;
            var day = StatisticsInterval.Day;
            var month = StatisticsInterval.Month;
            var quarter = StatisticsInterval.Quarter;
            var year = StatisticsInterval.Year;
            switch (compressSetting.CompressTo)
            {
                case StatisticsInterval.OneSecond:
                    recordsQuery = recordsQuery.Where(x => false);
                    break;
                case StatisticsInterval.TenSeconds:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval == oneSecond || x.StatisticsInterval == tenSeconds);
                    break;
                case StatisticsInterval.OneMinute:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval == oneSecond || x.StatisticsInterval == tenSeconds || x.StatisticsInterval == oneMinute);
                    break;
                case StatisticsInterval.FiveMinutes:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval == oneSecond || x.StatisticsInterval == tenSeconds || x.StatisticsInterval == oneMinute || x.StatisticsInterval == fiveMinutes);
                    break;
                case StatisticsInterval.TenMinutes:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval == oneSecond || x.StatisticsInterval == tenSeconds || x.StatisticsInterval == oneMinute || x.StatisticsInterval == fiveMinutes || x.StatisticsInterval == tenMinutes);
                    break;
                case StatisticsInterval.HalfAnHour:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval != hour && x.StatisticsInterval != day && x.StatisticsInterval != month && x.StatisticsInterval != quarter && x.StatisticsInterval != year);
                    break;
                case StatisticsInterval.Hour:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval != day && x.StatisticsInterval != month && x.StatisticsInterval != quarter && x.StatisticsInterval != year);
                    break;
                case StatisticsInterval.Day:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval != month && x.StatisticsInterval != quarter && x.StatisticsInterval != year);
                    break;
                case StatisticsInterval.Month:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval != quarter && x.StatisticsInterval != year);
                    break;
                case StatisticsInterval.Quarter:
                    recordsQuery = recordsQuery.Where(x => x.StatisticsInterval != year);
                    break;
                case StatisticsInterval.Year:
                    recordsQuery = recordsQuery.Where(x => true);
                    break;
                default:
                    throw new Exception("Неизвестный интервал статистики: " + compressSetting.CompressTo.ToString());
            }

            recordsQuery = recordsQuery.Where(x => x.StatisticsSetting.__PrimaryKey == compressSetting.StatisticsSetting.__PrimaryKey && x.To <= timeEnd).OrderBy(x => x.Since);
            if (MaxRecordsForOneCompression > 0)
                recordsQuery = recordsQuery.Take(MaxRecordsForOneCompression);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = recordsQuery.ToList();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(compressSetting.StatisticsSetting.Subscription, (int)time, "DefaultStatisticsCompressorService.LoadRecordsForCompression() load StatRecords.");

            return dobjs;
        }

        private List<StatisticsRecord> CompressRecords(IEnumerable<StatisticsRecord> records, StatisticsCompressionSetting compressSetting)
        {
            // Формирование списка новых сжатых записей.
            var compressedRecords = records
                .GroupBy(x => GetStartOfInterval(x.Since, compressSetting.CompressTo))
                .Select(x =>
                    new StatisticsRecord()
                    {
                        StatisticsSetting = compressSetting.StatisticsSetting,
                        Since = x.Key,
                        To = AddStatIntervalToDate(x.Key, compressSetting.CompressTo, 1),
                        StatisticsInterval = compressSetting.CompressTo,
                        SentCount = x.Sum(y => y.SentCount),
                        ReceivedCount = x.Sum(y => y.ReceivedCount),
                        ErrorsCount = x.Sum(y => y.ErrorsCount),
                        UniqueErrorsCount = x.OrderBy(y => y.Since).Last().UniqueErrorsCount,
                        QueueLength = x.OrderBy(y => y.Since).Last().QueueLength,
                        SentAvgTime = x.Sum(y => y.SentAvgTime) / x.Count(),
                        QueryAvgTime = x.Sum(y => y.QueryAvgTime) / x.Count(),
                        ConnectionCount = x.OrderBy(y => y.Since).Last().ConnectionCount,
                    })
                .ToList();

            return compressedRecords;
        }

        /// <summary>
        /// Подготавливает интервалы, вычисляя дополнительные поля и удаляя пустые интервалы.
        /// </summary>
        /// <param name="now">Время начала процесса сжатия статистики.</param>
        /// <param name="compressionSettings">Список конфигураций сжатия статистики (CompressionSetting).</param>
        /// <returns>Список подготовленных интервалов.</returns>
        private List<PreparedInterval> PrepareIntervals(DateTime now, List<StatisticsCompressionSetting> compressionSettings)
        {
            if (!compressionSettings.Any())
                return new List<PreparedInterval>();

            var intervals = compressionSettings
                .Select(x =>
                    {
                        var i = new PreparedInterval();
                        i.Setting = x;
                        i.TimeLimit = AddTimeUnitsToDate(now, x.StatisticsAgeUnits, -x.StatisticsAgeCount);
                        switch (x.CompressTo)
                        {
                            case StatisticsInterval.OneSecond:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute, i.TimeLimit.Second);
                                break;
                            case StatisticsInterval.TenSeconds:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute, i.TimeLimit.Second / 10 * 10);
                                break;
                            case StatisticsInterval.OneMinute:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute, 0);
                                break;
                            case StatisticsInterval.FiveMinutes:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute / 5 * 5, 0);
                                break;
                            case StatisticsInterval.TenMinutes:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute / 10 * 10, 0);
                                break;
                            case StatisticsInterval.HalfAnHour:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, i.TimeLimit.Minute / 30 * 30, 0);
                                break;
                            case StatisticsInterval.Hour:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, i.TimeLimit.Hour, 0, 0);
                                break;
                            case StatisticsInterval.Day:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, i.TimeLimit.Day, 0, 0, 0);
                                break;
                            case StatisticsInterval.Month:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month, 0, 0, 0, 0);
                                break;
                            case StatisticsInterval.Quarter:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, i.TimeLimit.Month / 3 * 3, 0, 0, 0, 0);
                                break;
                            case StatisticsInterval.Year:
                                i.IntervalEnd = new DateTime(i.TimeLimit.Year, 0, 0, 0, 0, 0);
                                break;
                            default:
                                throw new Exception("Неизвестный интервал статистики: " + x.CompressTo.ToString());
                        }
                        return i;
                    })
                .OrderBy(x => x.TimeLimit)
                .ToList();

            var preparedIntervals = new List<PreparedInterval>();
            preparedIntervals.Add(intervals[0]);
            for (var i = 1; i < intervals.Count; i++)
            {
                intervals[i].IntervalStart = intervals[i - 1].IntervalEnd;

                // В результат должны попасть только интервалы, в которые умещается хотя-бы одна итерация.
                if (intervals[i].IntervalStart <= AddStatIntervalToDate(intervals[i].IntervalEnd, intervals[i].Setting.CompressTo, -1))
                    preparedIntervals.Add(intervals[i]);
            }

            return preparedIntervals;
        }

        /// <summary>
        /// Выполняет сжатие записей из заданного подготовленного интервала.
        /// </summary>
        /// <param name="now">Время начала процесса сжатия статистики.</param>
        /// <param name="interval">Подготовленный интервал.</param>
        private void CompressRecords(DateTime now, PreparedInterval interval)
        {
            var records = LoadRecordsForCompression(interval);

            if (!records.Any())
                return;


        }

        /// <summary>
        /// Загружает подлежащие сжатию записи.
        /// </summary>
        /// <param name="interval">Подготовленный интервал, записи из которого нужно загрузить.</param>
        /// <returns>Список записей статистики, загруженный из БД.</returns>
        private List<StatisticsRecord> LoadRecordsForCompression(PreparedInterval interval)
        {
            var recordsQuery = _dataService.Query<StatisticsRecord>(StatisticsRecord.Views.CompressView);

            // Если указано начало интервала, нужно добавить дополнительное условие.
            if (interval.IntervalStart != null)
                recordsQuery = recordsQuery.Where(x => x.Since >= interval.IntervalStart);

            // Добавление основного условия к выборке.
            // Выбираются только записи, интервал которых меньше целевого.
            var timeEnd = interval.IntervalEnd;
            switch (interval.Setting.CompressTo)
            {
                case StatisticsInterval.OneSecond:
                    recordsQuery = recordsQuery.Where(x => false);
                    break;
                case StatisticsInterval.TenSeconds:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval == StatisticsInterval.OneSecond);
                    break;
                case StatisticsInterval.OneMinute:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && (x.StatisticsInterval == StatisticsInterval.OneSecond || x.StatisticsInterval == StatisticsInterval.TenSeconds));
                    break;
                case StatisticsInterval.FiveMinutes:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && (x.StatisticsInterval == StatisticsInterval.OneSecond || x.StatisticsInterval == StatisticsInterval.TenSeconds || x.StatisticsInterval == StatisticsInterval.OneMinute));
                    break;
                case StatisticsInterval.TenMinutes:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && (x.StatisticsInterval == StatisticsInterval.OneSecond || x.StatisticsInterval == StatisticsInterval.TenSeconds || x.StatisticsInterval == StatisticsInterval.OneMinute || x.StatisticsInterval == StatisticsInterval.FiveMinutes));
                    break;
                case StatisticsInterval.HalfAnHour:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && (x.StatisticsInterval == StatisticsInterval.OneSecond || x.StatisticsInterval == StatisticsInterval.TenSeconds || x.StatisticsInterval == StatisticsInterval.OneMinute || x.StatisticsInterval == StatisticsInterval.FiveMinutes || x.StatisticsInterval == StatisticsInterval.TenMinutes));
                    break;
                case StatisticsInterval.Hour:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval != StatisticsInterval.Hour && x.StatisticsInterval != StatisticsInterval.Day && x.StatisticsInterval != StatisticsInterval.Month && x.StatisticsInterval != StatisticsInterval.Quarter && x.StatisticsInterval != StatisticsInterval.Year);
                    break;
                case StatisticsInterval.Day:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval != StatisticsInterval.Day && x.StatisticsInterval != StatisticsInterval.Month && x.StatisticsInterval != StatisticsInterval.Quarter && x.StatisticsInterval != StatisticsInterval.Year);
                    break;
                case StatisticsInterval.Month:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval != StatisticsInterval.Month && x.StatisticsInterval != StatisticsInterval.Quarter && x.StatisticsInterval != StatisticsInterval.Year);
                    break;
                case StatisticsInterval.Quarter:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval != StatisticsInterval.Quarter && x.StatisticsInterval != StatisticsInterval.Year);
                    break;
                case StatisticsInterval.Year:
                    recordsQuery = recordsQuery.Where(x => x.To <= timeEnd && x.StatisticsInterval != StatisticsInterval.Year);
                    break;
                default:
                    throw new Exception("Неизвестный интервал статистики: " + interval.Setting.CompressTo.ToString());
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = recordsQuery.ToList();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(interval.Setting.StatisticsSetting.Subscription, (int)time, "DefaultStatisticsCompressorService.LoadRecordsForCompression() load StatRecords.");

            return dobjs;
        }

        private DateTime GetStartOfInterval(DateTime date, StatisticsInterval intervalType)
        {
            switch (intervalType)
            {
                case StatisticsInterval.OneSecond:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                case StatisticsInterval.TenSeconds:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second / 10 * 10);
                case StatisticsInterval.OneMinute:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                case StatisticsInterval.FiveMinutes:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute / 5 * 5, 0);
                case StatisticsInterval.TenMinutes:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute / 10 * 10, 0);
                case StatisticsInterval.HalfAnHour:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute / 30 * 30, 0);
                case StatisticsInterval.Hour:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                case StatisticsInterval.Day:
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                case StatisticsInterval.Month:
                    return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
                case StatisticsInterval.Quarter:
                    return new DateTime(date.Year, (date.Month - 1) / 3 * 3 + 1, 1, 0, 0, 0);
                case StatisticsInterval.Year:
                    return new DateTime(date.Year, 1, 1, 0, 0, 0);
                default:
                    throw new Exception("Неизвестный интервал статистики: " + intervalType.ToString());
            }
        }

        /// <summary>
        /// Увеличивает время на указанное число единиц измерения.
        /// </summary>
        /// <param name="date">Исходное время.</param>
        /// <param name="unit">Единица измерения.</param>
        /// <param name="unitsNum">Число единиц измерения, на которое требуется изменить исходное время.</param>
        /// <returns>Полученное время.</returns>
        private DateTime AddTimeUnitsToDate(DateTime date, TimeUnit unit, int unitsNum)
        {
            switch (unit)
            {
                case TimeUnit.Minute:
                    return date.AddMinutes(unitsNum);
                case TimeUnit.Hour:
                    return date.AddHours(unitsNum);
                case TimeUnit.Day:
                    return date.AddDays(unitsNum);
                case TimeUnit.Month:
                    return date.AddMonths(unitsNum);
                case TimeUnit.Year:
                    return date.AddYears(unitsNum);
                default:
                    throw new Exception("Неизвестная единица измерения: " + unit.ToString());
            }
        }

        /// <summary>
        /// Увеличивает время на указанное число интервалов статистики.
        /// </summary>
        /// <param name="date">Исходное время.</param>
        /// <param name="intervalType">Тип интервала статистики.</param>
        /// <param name="intervalsNum">Число интервалов статистики, на которое требуется изменить исходное время.</param>
        /// <returns>Полученное время.</returns>
        private DateTime AddStatIntervalToDate(DateTime date, StatisticsInterval intervalType, int intervalsNum)
        {
            switch (intervalType)
            {
                case StatisticsInterval.OneSecond:
                    return date.AddSeconds(intervalsNum);
                case StatisticsInterval.TenSeconds:
                    return date.AddSeconds(intervalsNum * 10);
                case StatisticsInterval.OneMinute:
                    return date.AddMinutes(intervalsNum);
                case StatisticsInterval.FiveMinutes:
                    return date.AddMinutes(intervalsNum * 5);
                case StatisticsInterval.TenMinutes:
                    return date.AddMinutes(intervalsNum * 10);
                case StatisticsInterval.HalfAnHour:
                    return date.AddMinutes(intervalsNum * 30);
                case StatisticsInterval.Hour:
                    return date.AddHours(intervalsNum);
                case StatisticsInterval.Day:
                    return date.AddDays(intervalsNum);
                case StatisticsInterval.Month:
                    return date.AddMonths(intervalsNum);
                case StatisticsInterval.Quarter:
                    return date.AddMonths(intervalsNum * 3);
                case StatisticsInterval.Year:
                    return date.AddYears(intervalsNum);
                default:
                    throw new Exception("Неизвестный интервал статистики: " + intervalType.ToString());
            }
        }
    }
}
