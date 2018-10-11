namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.ServiceModel;
    using ClientTools;
    using Components;

    /// <summary>
    /// Sender of callback messages through WCF.
    /// <para>
    /// Configuration file must contain endpoint “HighwaySbWcf.ICallbackSubscriber” at client section.
    /// </para>
    /// </summary>
    public class WcfMessageSender : IMessageSender
    {
        /// <summary>
        /// Current logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Конструктор, инициализирующий свойства объекта для отправки сообщений.
        /// </summary>
        /// <param name="client">Получатель сообщений.</param>
        public WcfMessageSender(Client client, ILogger logger)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            Client = client;
        }

        /// <summary>
        /// Клиент, которому будут отправляться сообщения с помощью текущего экземпляра.
        /// </summary>
        public Client Client { get; private set; }

        /// <summary>
        /// Отправить сообщение.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно отправить.</param>
        /// <returns>Успешно ли было отправлено сообщение.</returns>
        public bool SendMessage(Message message)
        {
            if (string.IsNullOrEmpty(Client.Address))
            {
                _logger.LogError("Ошибка отправки сообщения клиенту через WCF", $"У клиента '{Client.Name ?? Client.ID}' не указан адрес для отправки сообщений.", message);
                return false;
            }

            var channelFactory = new ChannelFactory<IServiceBusCallbackClient>(
                "CallbackClient",
                Client.DnsIdentity != null ? new EndpointAddress(new Uri(Client.Address), EndpointIdentity.CreateDnsIdentity(Client.DnsIdentity)) : new EndpointAddress(Client.Address));
            IServiceBusCallbackClient channel = channelFactory.CreateChannel();
            ((IClientChannel)channel).Open();

            ServiceBusMessage messageFromEsb = ServiceHelper.CreateWcfMessageFromEsb(
                message.ReceivingTime,
                message.MessageType.ID,
                message.Body,
                message.Sender,
                message.Group,
                ServiceHelper.GetTagDictionary(message),
                message.BinaryAttachment);

            return ServiceHelper.TryWithExceptionLogging(
                () => channel.AcceptMessage(messageFromEsb),
                () => {
                    ((IClientChannel)channel).Close();
                    channelFactory.Close();
                },
                string.Format("Ошибка отправки сообщения клиенту через WCF по адресу {0}", Client.Address),
                Client,
                message,
                _logger);
        }
    }
}
