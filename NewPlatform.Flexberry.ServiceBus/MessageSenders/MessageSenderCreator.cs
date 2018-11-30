namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;

    using NewPlatform.Flexberry.ServiceBus.Components;

    /// <summary>
    /// Class for creating message senders.
    /// </summary>
    public class MessageSenderCreator
    {
        /// <summary>
        /// Logger for logging.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// If <c>true</c>, legacy types of message senders will be created.
        /// </summary>
        private readonly bool useLegacySenders;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSenderCreator"/> class.
        /// </summary>
        /// <param name="logger">Logger for logging.</param>
        /// <param name="useLegacySenders">If <c>true</c>, legacy types of message senders will be created.</param>
        public MessageSenderCreator(ILogger logger, bool useLegacySenders)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.useLegacySenders = useLegacySenders;
        }

        /// <summary>
        /// Creates a message sender for the specified <paramref name="subscription"/>.
        /// </summary>
        /// <param name="subscription">Subscription on which the message will be sent.</param>
        /// <returns>Message sender.</returns>
        public IMessageSender GetMessageSender(Subscription subscription)
        {
            switch (subscription.TransportType)
            {
                case TransportType.MAIL:
                    return new MailMessageSender(subscription.Client, logger);

                case TransportType.HTTP:
                    if (useLegacySenders)
                    {
                        return new LegacyHttpMessageSender(subscription.Client, logger);
                    }

                    return new HttpMessageSender(subscription.Client, logger);

                case TransportType.WEB:
                    if (useLegacySenders)
                    {
                        return new LegacyWebMessageSender(subscription.Client, logger);
                    }

                    return new WebMessageSender(subscription.Client, logger);

                case TransportType.WCF:
                    if (useLegacySenders)
                    {
                        return new LegacyWcfMessageSender(subscription.Client, logger);
                    }

                    return new WcfMessageSender(subscription.Client, logger);

                default:
                    throw new ArgumentException($"Unknown transport type: {subscription.TransportType}.");
            }
        }
    }
}
