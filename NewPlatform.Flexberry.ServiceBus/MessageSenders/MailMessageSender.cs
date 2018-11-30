namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.Configuration;

    using NewPlatform.Flexberry.ServiceBus.Components;
    using NewPlatform.Flexberry.ServiceBus.Mail;

    /// <summary>
    /// A class that implements sending messages to clients via email.
    /// </summary>
    public class MailMessageSender : BaseMessageSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailMessageSender"/> class.
        /// </summary>
        /// <param name="client">Client, recipient of messages.</param>
        /// <param name="logger">Logger for logging.</param>
        public MailMessageSender(Client client, ILogger logger)
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
                Logger.LogError("Error sending email to client.", $"The client '{Client.Name ?? Client.ID}' not specified address to send messages.", message);
                return false;
            }

            var mailMessage = new ForMailMessage()
            {
                MsgTypeID = message.MessageType.ID,
                Body = message.Body,
                Attachment = message.BinaryAttachment,
                ClientID = message.Sender,
                Group = message.Group,
                Tags = ServiceHelper.GetTagDictionary(message),
            };

            return ServiceHelper.TryWithExceptionLogging(
                () => mailMessage.Send(Client.Address, ConfigurationManager.AppSettings["MailLogin"], ConfigurationManager.AppSettings["MailServer"]),
                null,
                $"Error sending a message to the client '{Client.Name ?? Client.ID}' by e-mail to the address '{Client.Address}'.",
                Client,
                message,
                Logger);
        }
    }
}
