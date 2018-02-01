namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    /// <summary>
    /// Интерфейс, описывающий объект для отправки сообщений конкретному клиенту.
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Клиент, которому будут отправляться сообщения с помощью текущего экземпляра.
        /// </summary>
        Client Client { get; }

        /// <summary>
        /// Отправить сообщение.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно отправить.</param>
        /// <returns>Успешно ли было отправлено сообщение.</returns>
        bool SendMessage(Message message);
    }
}