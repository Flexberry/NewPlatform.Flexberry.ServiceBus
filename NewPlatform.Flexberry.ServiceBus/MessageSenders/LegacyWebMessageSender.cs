namespace NewPlatform.Flexberry.ServiceBus.MessageSenders
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text.RegularExpressions;

    using NewPlatform.Flexberry.ServiceBus.ClientTools;
    using NewPlatform.Flexberry.ServiceBus.Components;

    using Message = NewPlatform.Flexberry.ServiceBus.Message;

    /// <summary>
    /// A class that implements sending messages to clients through the ASMX client service.
    /// <para>Sends an <see cref="MessageFromESB"/> using the <see cref="ICallbackSubscriber"/> interface.</para>
    /// </summary>
    public class LegacyWebMessageSender : BaseMessageSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyWebMessageSender"/> class.
        /// </summary>
        /// <param name="client">Client, recipient of messages.</param>
        /// <param name="logger">Logger for logging.</param>
        public LegacyWebMessageSender(Client client, ILogger logger)
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
                Logger.LogError("Error sending message to client via WEB.", $"The client '{Client.Name ?? Client.ID}' not specified address to send messages.", message);
                return false;
            }

            ChannelFactory<ICallbackSubscriber> channelFactory;
            ICallbackSubscriber channel;
            try
            {
                var endpointAddress = new EndpointAddress(new Uri(Client.Address), AddressHeader.CreateAddressHeader("headerName", Regex.Replace(Client.Address, ".asmx$", string.Empty), "headerValue"));
                channelFactory = new ChannelFactory<ICallbackSubscriber>(new BasicHttpBinding(), endpointAddress);
                channel = channelFactory.CreateChannel();
                ((IClientChannel)channel).Open();
            }
            catch (Exception exception)
            {
                Logger.LogUnhandledException(exception, message);
                throw exception;
            }

            var sbMessage = new MessageFromESB()
            {
                MessageFormingTime = message.ReceivingTime,
                MessageTypeID = message.MessageType.ID,
                Body = message.Body,
                Attachment = message.BinaryAttachment,
                SenderName = message.Sender,
                GroupID = message.Group,
                Tags = ServiceHelper.GetTagDictionary(message),
            };

            return ServiceHelper.TryWithExceptionLogging(
                () => channel.AcceptMessage(sbMessage),
                () =>
                {
                    ((IClientChannel)channel).Close();
                    channelFactory.Close();
                },
                $"Error sending message to the client '{Client.Name ?? Client.ID}' via WEB at address '{Client.Address}'.",
                Client,
                message,
                Logger);
        }
    }
}
