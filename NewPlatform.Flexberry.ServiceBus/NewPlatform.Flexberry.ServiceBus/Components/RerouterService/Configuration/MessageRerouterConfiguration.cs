namespace NewPlatform.Flexberry.ServiceBus.Components.RerouterConfiguration
{
    using System.Configuration;

    /// <summary>
    /// Раздел конфигурации приложения.
    /// </summary>
    public class MessageRerouterConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("serverPort")]
        public int ServerPort
        {
            get { return (int)base["serverPort"]; }
        }

        [ConfigurationProperty("timeout")]
        public int Timeout
        {
            get { return (int)base["timeout"]; }
        }

        [ConfigurationProperty("receiverId")]
        public string ReceiverId
        {
            get { return (string)base["receiverId"]; }
        }

        [ConfigurationProperty("senderId")]
        public string SenderId
        {
            get { return (string)base["senderId"]; }
        }

        [ConfigurationProperty("recievers")]
        public RecieverElementCollection Recievers
        {
            get { return (RecieverElementCollection)base["recievers"]; }
        }

        /// <summary>
        /// Получает из секцию конфигурации из файла конфигурации.
        /// </summary>
        public static MessageRerouterConfiguration Current
        {
            get
            {
                var section = ConfigurationManager.GetSection("messageRerouterConfiguration");
                if (section == null)
                    throw new ConfigurationErrorsException("Секция конфигурации \"messageRerouterConfiguration\" не найдена.");
                return (MessageRerouterConfiguration)section;
            }
        }
    }
}
