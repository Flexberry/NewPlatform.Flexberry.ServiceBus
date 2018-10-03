namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    internal class MessageSenderCreator
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Создание экземпляра класса <see cref="MessageSenderCreator"/>
        /// </summary>
        /// <param name="logger">Компонент для логирования.</param>
        public MessageSenderCreator(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Получить объект для отправки сообщений, тип которого зависит от метода отправки.
        /// </summary>
        /// <param name="subscription">Подписка, для которой нужно получить объект для отправки.</param>
        /// <returns>Объект для отправки сообщений.</returns>
        public IMessageSender GetMessageSender(Subscription subscription)
        {
            IMessageSender result;
            switch (subscription.TransportType)
            {
                case TransportType.HTTP:
                    result = new HttpMessageSender(subscription.Client, _logger);
                    break;
                case TransportType.MAIL:
                    result = new MailMessageSender(subscription.Client, _logger);
                    break;
                case TransportType.WCF:
                    result = new WcfMessageSender(subscription.Client, _logger);
                    break;
                case TransportType.WEB:
                    result = new WebMessageSender(subscription.Client, _logger);
                    break;
                default:
                    throw new ArgumentException("Неизвестный способ отправки сообщения.");
            }

            return result;
        }
    }
}
