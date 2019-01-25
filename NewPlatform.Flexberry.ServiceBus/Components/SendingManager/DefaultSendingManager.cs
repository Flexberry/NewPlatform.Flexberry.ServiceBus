namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using ICSSoft.STORMNET.Business;
    using MultiTasking;

    /// <summary>
    /// Класс, использующийся для отправки сообщений по умолчанию.
    /// </summary>
    internal class DefaultSendingManager : BaseSendingManager
    {
        private readonly ILogger _logger;

        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly IStatisticsService _statistics;

        private readonly IDataService _dataService;

        private readonly Thread _scanningThread;

        private readonly ManualResetEvent _scanninWaitEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _scanningStoppedEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent _scanningStartedEvent = new ManualResetEvent(false);

        private volatile bool _requestStopScanning;

        public DefaultSendingManager(ISubscriptionsManager subscriptionsManager, IStatisticsService statistics, IDataService dataService, ILogger logger)
            : base(subscriptionsManager, statistics, dataService, logger, true)
        {
            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (statistics == null)
                throw new ArgumentNullException(nameof(statistics));

            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            _subscriptionsManager = subscriptionsManager;
            _statistics = statistics;
            _dataService = dataService;

            _scanningThread = new Thread(ScanMessages) { Name = "DefaultSendingManager.ScanMessages" };
        }

        /// <summary>
        /// Метод, выполняющий запуск потоков отправки для каждой подписки.
        /// </summary>
        private void SendMsgsByCallBack()
        {
            try
            {
                // Ищем актуальные подписки, для которых есть сообщения.
                IEnumerable<Subscription> callbackSubscriptions = _subscriptionsManager.GetCallbackSubscriptions().Where(x => GetCurrentMessageCount(x.Client.ID, x.MessageType.ID) > 0);
                try
                {
                    foreach (var subscription in callbackSubscriptions)
                        SubscriberThreadPool.QueueUserWorkItem(subscription, _scanninWaitEvent, _statistics, _dataService, _logger);

                    SubscriberThreadPool.CheckForUnActiveSubscribers();
                }
                catch (Exception e)
                {
                    _logger.LogUnhandledException(e, null, "Ошибка при порождении потоков подписчиков через QueueUserWorkItem.");
                }
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e, null, "Ошибка при получении активных подписок, отправляемых по Callback.");
            }
        }

        /// <summary>
        /// Поток сканирующий БД на наличие подписок.
        /// </summary>
        private void ScanMessages()
        {
            try
            {
                // Notify main thread that scanning thread has started.
                _scanningStartedEvent.Set();

                while (true)
                {
                    if (_requestStopScanning)
                    {
                        SubscriberThreadPool.ReleaseAllSubscriptionThreads();
                        return;
                    }

                    try
                    {
                        SendMsgsByCallBack();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogUnhandledException(ex);
                    }

                    _scanninWaitEvent.WaitOne(ScanningPeriodMilliseconds);
                }
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e);
            }
            finally
            {
                // Notify main thread that scanning thread has stopped.
                _scanningStoppedEvent.Set();
            }
        }

        /// <summary>
        /// Добавить сообщение в очередь для отправки.
        /// </summary>
        /// <param name="msg">Сообщение, которое нужно отправить.</param>
        public override void QueueForSending(Message msg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Начать работу.
        /// </summary>
        public override void Start()
        {
            // Starting scanning thread and waiting it to start.
            _scanningThread.Start();
            _scanningStartedEvent.WaitOne();

            base.Start();
        }

        /// <summary>
        /// Остановить работу.
        /// </summary>
        public override void Stop()
        {
            // Requesting scanning thread to stop and wake it up.
            _requestStopScanning = true;
            _scanninWaitEvent.Set();

            // Waiting scanning thread to stop.
            _scanningStoppedEvent.WaitOne();

            base.Stop();
        }

        public override void AfterStop()
        {
            Dispose(true);

            base.AfterStop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanningStartedEvent?.Dispose();
                _scanningStoppedEvent?.Dispose();
                _scanninWaitEvent?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
