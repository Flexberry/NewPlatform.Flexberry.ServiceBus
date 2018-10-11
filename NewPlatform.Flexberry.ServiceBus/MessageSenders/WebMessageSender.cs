namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text.RegularExpressions;
    using ClientTools;
    using Components;

    /// <summary>
    /// Класс, реализующий отправку сообщений клиентам путем обращения к asmx-сервису.
    /// </summary>
    public class WebMessageSender : IMessageSender
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Клиент, которому будут отправляться сообщения с помощью текущего экземпляра.
        /// </summary>
        public Client Client { get; private set; }

        /// <summary>
        /// Конструктор, инициализирующий свойства объекта для отправки сообщений.
        /// </summary>
        /// <param name="client">Получатель сообщений.</param>
        public WebMessageSender(Client client, ILogger logger)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            Client = client;
        }

        /// <summary>
        /// Отправить сообщение.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно отправить.</param>
        /// <returns>Успешно ли было отправлено сообщение.</returns>
        public bool SendMessage(Flexberry.ServiceBus.Message message)
        {
            if (string.IsNullOrEmpty(Client.Address))
            {
                _logger.LogError(
                    "Ошибка отправки сообщения клиенту через веб-сервис",
                    $"У клиента '{Client.Name ?? Client.ID}' не указан адрес для отправки сообщений.",
                    message);
                return false;
            }

            var channelFactory =
                new ChannelFactory<IServiceBusCallbackClient>(
                    new BasicHttpBinding(),
                    new EndpointAddress(
                        new Uri(Client.Address),
                        AddressHeader.CreateAddressHeader("headerName", Regex.Replace(Client.Address, ".asmx$", string.Empty), "headerValue")));

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

            return ServiceHelper.TryWithExceptionLogging(() => channel.AcceptMessage(messageFromEsb), null, null, null, null, _logger);
        }
    }
}
