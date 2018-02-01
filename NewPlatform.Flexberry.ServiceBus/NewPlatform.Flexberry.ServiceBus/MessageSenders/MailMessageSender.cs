namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.Configuration;
    using Components;
    using Mail;

    /// <summary>
    /// Класс, реализующий отправку сообщений клиентам по электронной почте.
    /// </summary>
    public class MailMessageSender : IMessageSender
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Клиент, которому будут отправляться сообщения с помощью текущего экземпляра.
        /// </summary>
        public Client Client { get; }

        /// <summary>
        /// Конструктор, инициализирующий свойства объекта для отправки сообщений.
        /// </summary>
        /// <param name="client">Получатель сообщений.</param>
        public MailMessageSender(Client client, ILogger logger)
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
        public bool SendMessage(Message message)
        {
            if (string.IsNullOrEmpty(Client.Address))
            {
                _logger.LogError(
                    "Ошибка отправки сообщения клиенту по электронной почте",
                    $"У клиента '{Client.Name ?? Client.ID}' не указан адрес для отправки сообщений.",
                    message);
                return false;
            }

            var mailMessage = new ForMailMessage()
            {
                Attachment = message.BinaryAttachment,
                Body = message.Body,
                ClientID = message.Sender,
                Group = message.Group,
                MsgTypeID = message.MessageType.ID,
                Tags = ServiceHelper.GetTagDictionary(message)
            };

            return ServiceHelper.TryWithExceptionLogging(
                () => mailMessage.Send(Client.Address, ConfigurationManager.AppSettings["MailLogin"], ConfigurationManager.AppSettings["MailServer"]),
                null,
                "Ошибка отправки сообщения клиенту по электронной почте",
                Client,
                message,
                _logger);
        }
    }
}
