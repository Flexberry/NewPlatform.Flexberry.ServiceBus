#if NETFRAMEWORK
namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Threading;
    using NewPlatform.Flexberry.ServiceBus.Mail;

    /// <summary>
    /// Implementation of service for scanning e-mails to get new messages.
    /// </summary>
    internal class MailScanningService : BaseServiceBusComponent, IMailScanningService
    {
        private readonly MailScanningServiceSettings _settings;

        private readonly IReceivingManager _receivingManager;

        private readonly ILogger _logger;

        private Thread _scanMailThread;

        private readonly ManualResetEvent _stopWaitHandle = new ManualResetEvent(false);

        public MailScanningService(MailScanningServiceSettings settings, IReceivingManager receivingManager, ILogger logger)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _settings = settings;
            _receivingManager = receivingManager;
            _logger = logger;
        }

        public override void Start()
        {
            if (_settings.CheckMail)
            {
                _scanMailThread = new Thread(ScanMessages);
                _scanMailThread.Start();
            }
        }

        public override void Stop()
        {
            if (_settings.CheckMail)
                _scanMailThread.Abort();
        }

        /// <summary>
        /// Метод для потока, выполняющего сканирование электронной почты на наличие новых сообщений.
        /// </summary>
        private void ScanMessages()
        {
            try
            {
                while (true)
                {
                    MailUtils.ReceiveMess(_receivingManager, _logger);
                    if (_stopWaitHandle.WaitOne(_settings.MailScanPeriod))
                        return;
                }
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e);
            }
        }
    }
}
#endif
