namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.Net;
    using System.Text;
    using Components;
    using Newtonsoft.Json;

    /// <summary>
    /// Класс, реализующий отправку сообщений посредством HTTP-запросов.
    /// </summary>
    public class HttpMessageSender : IMessageSender
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
        /// <param name="logger"></param>
        public HttpMessageSender(Client client, ILogger logger)
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
                    "Ошибка отправки сообщения клиенту через HTTP",
                    $"У клиента '{Client.Name ?? Client.ID}' не указан адрес для отправки сообщений.",
                    message);
                return false;
            }

            string url = string.Format("{0}/Message", Client.Address.TrimEnd('/'));
            string json = JsonConvert.SerializeObject(
                ServiceHelper.CreateHttpMessageFromEsb(
                message.__PrimaryKey.ToString(),
                message.ReceivingTime,
                message.MessageType.ID,
                message.Body,
                message.Sender,
                message.Group,
                ServiceHelper.GetTagDictionary(message),
                message.BinaryAttachment));

            Action sendMessage = () =>
            {
                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    webClient.UploadString(url, "POST", json);
                }
            };

            return ServiceHelper.TryWithExceptionLogging(
                sendMessage,
                null,
                $"Ошибка отправки сообщения клиенту по HTTP по адресу {Client.Address}",
                Client,
                message,
                _logger);
        }
    }
}
