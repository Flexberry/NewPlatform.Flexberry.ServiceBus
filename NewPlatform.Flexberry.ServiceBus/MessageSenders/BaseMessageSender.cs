namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;

    using NewPlatform.Flexberry.ServiceBus.Components;

    /// <summary>
    /// The base class of the message sender.
    /// </summary>
    public abstract class BaseMessageSender : IMessageSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMessageSender"/> class.
        /// </summary>
        /// <param name="client">Client, recipient of messages.</param>
        /// <param name="logger">Logger for logging.</param>
        public BaseMessageSender(Client client, ILogger logger)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets client, recipient of messages.
        /// </summary>
        public Client Client { get; }

        /// <summary>
        /// Gets logger for logging.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Send message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Whether the message was sent.</returns>
        public abstract bool SendMessage(Message message);
    }
}
