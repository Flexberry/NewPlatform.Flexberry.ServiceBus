namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.Net;
    using System.Text;

    using NewPlatform.Flexberry.ServiceBus.Components;

    using Newtonsoft.Json;

    /// <summary>
    /// A class that implements sending messages to clients via HTTP.
    /// <para>The request body is formed using the class <see cref="HttpMessageFromEsb"/>.</para>
    /// </summary>
    public class LegacyHttpMessageSender : BaseMessageSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyHttpMessageSender"/> class.
        /// </summary>
        /// <param name="client">Client, recipient of messages.</param>
        /// <param name="logger">Logger for logging.</param>
        public LegacyHttpMessageSender(Client client, ILogger logger)
            : base(client, logger)
        {
        }

        /// <inheritdoc/>
        public override bool SendMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(Client.Address))
            {
                Logger.LogError("Error sending message to client via HTTP.", $"The client '{Client.Name ?? Client.ID}' not specified address to send messages.", message);
                return false;
            }

            string url = string.Format("{0}/Message", Client.Address.TrimEnd('/'));
            string json = JsonConvert.SerializeObject(new HttpMessageFromEsb()
            {
                Id = message.__PrimaryKey.ToString(),
                MessageFormingTime = message.ReceivingTime,
                MessageTypeID = message.MessageType.ID,
                Body = message.Body,
                Attachment = message.BinaryAttachment,
                SenderName = message.Sender,
                GroupID = message.Group,
                Tags = ServiceHelper.GetTagDictionary(message),
            });

            return ServiceHelper.TryWithExceptionLogging(
                () =>
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        webClient.Encoding = Encoding.UTF8;
                        webClient.UploadString(url, "POST", json);
                    }
                },
                null,
                $"Error sending message to the client '{Client.Name ?? Client.ID}' via HTTP at address '{Client.Address}'.",
                Client,
                message,
                Logger);
        }
    }
}
