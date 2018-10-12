namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Configuration;

    /// <summary>
    /// В данном классе шина выступает в роли клиента, принимая сообщения
    /// </summary>
    public partial class SBService : IServiceBusCallbackClient
    {
        /// <summary>
        /// Принять сообщение от шины.
        /// </summary>
        /// <param name="message">Принимаемое сообщение.</param>
        void IServiceBusCallbackClient.AcceptMessage(ServiceBusMessage message)
        {
            var msgFor = new ServiceBusMessage
            {
                Attachment = message.Attachment,
                Body = message.Body,
                MessageTypeID = message.MessageTypeID,
                ClientID = GetLastSenderID(message),
                Tags = message.Tags
            };

            if (!msgFor.Tags.ContainsKey("senderName"))
            {
                msgFor.Tags.Add("senderName", message.SenderName);
            }

            if (message.Group == string.Empty)
            {
                (this as IServiceBusClient).SendMessage(msgFor);
            }
            else
            {
                (this as IServiceBusClient).SendMessageWithGroup(msgFor, message.Group);
            }
        }

        /// <summary>
        /// Получить идентификатор шины, выступающей в качестве клиента.
        /// </summary>
        /// <returns>ServiceBusClientKey.</returns>
        string IServiceBusCallbackClient.GetSourceId()
        {
            return ConfigurationManager.AppSettings["ServiceBusClientKey"];
        }

        /// <summary>
        /// Извлечь из тегов идентификатор последнего отправителя.
        /// </summary>
        /// <param name="message">Сообщение, из которого извлекается последний отправитель.</param>
        /// <returns>Идентификатор последнего отправителя.</returns>
        private static string GetLastSenderID(ServiceBusMessage message)
        {
            string[] strings = message.Tags["sendingWay"].Split('/');

            return strings[strings.Length - 1];
        }
    }
}