namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Configuration;

    internal class MailScanningServiceSettings : BaseServiceBusComponent, IMailScanningServiceSettings
    {
        /// <summary>
        /// If true then mail scanning service is enabled.
        /// </summary>
        public bool CheckMail { get; set; }

        /// <summary>
        /// Time interval (in milliseconds) to check mail.
        /// </summary>
        public int MailScanPeriod { get; set; }

        public MailScanningServiceSettings LoadFromConfig()
        {
            return new MailScanningServiceSettings
            {
                CheckMail = bool.Parse(ConfigurationManager.AppSettings["CheckMail"]),
                MailScanPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["MailScanPeriod"]) * 1000
            };
        }
    }
}