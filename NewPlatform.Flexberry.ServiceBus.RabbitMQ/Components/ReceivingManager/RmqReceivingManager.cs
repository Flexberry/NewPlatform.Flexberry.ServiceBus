namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Content;
    using RabbitMQ.Client.Exceptions;
    using RabbitMQ.Client.Framing;
    using System;
    using System.Configuration;

    /// <summary>
    /// Component for receiving ESB-model messages from clients to RabbitMQ.
    /// </summary>
    internal class RmqReceivingManager : BaseServiceBusComponent, IReceivingManager
    {
        private readonly ILogger _logger;
        private IMessageConverter _messageConverter;

        private AmqpNamingManager _namingManager;

        /// <summary>
        /// Getting connection factory for specified user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>New connection factory with setted username and password.</returns>
        protected IConnectionFactory GetConnectionFactoryForUser(string username, string password)
        {
            var connectionFactory = new ConnectionFactory
            {
                UserName = username,
                Password = password,
                Uri = this.RmqUri,
                VirtualHost = this.RmqVirtualHost,
                Protocol = this.RmqProtocol,
            };

            return connectionFactory;
        }

        /// <summary>
        /// RabbitMQ communication protocol. By default is's AMQP_0_9_1.
        /// </summary>
        public IProtocol RmqProtocol { get; set; } = new Protocol();

        /// <summary>
        /// RabbitMQ VirtualHost.
        /// </summary>
        public string RmqVirtualHost { get; set; } = "/";

        /// <summary>
        /// RabbitMQ endpoint.
        /// </summary>
        public Uri RmqUri { get; set; }

        public RmqReceivingManager(ILogger logger, IMessageConverter converter, Uri rmqUri)
        {
            this.RmqUri = rmqUri;
            _logger = logger;
            _messageConverter = converter;
            _namingManager = new AmqpNamingManager();
        }

        /// <summary>
        /// Creates and fills MapMessageBuilder for publish message into broker.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <param name="model">AMQP-model.</param>
        /// <returns>MapMessageBuilder with filled body header properties.</returns>
        private MapMessageBuilder BuildMessage(ServiceBusMessage message, IModel model)
        {
            var messageBuilder = new MapMessageBuilder(model);
            messageBuilder.Properties.DeliveryMode = 2;

            int normalizedPriority = (message.Priority < 0 ? 0 : 
                                      message.Priority > 9 ? 9 : 
                                      message.Priority);
            messageBuilder.Properties.Priority = Convert.ToByte(normalizedPriority);

            var bodyProps = _messageConverter.GetBodyProperties(message);
            foreach (var bodyProp in bodyProps)
            {
                messageBuilder.Body.Add(bodyProp.Key, bodyProp.Value);
            }

            var headerProps = _messageConverter.GetProperties(message);
            if (messageBuilder.Properties.Headers == null)
            {
                messageBuilder.Properties.Headers = headerProps;
            }
            else
            {
                foreach (var headerProp in headerProps)
                {
                    messageBuilder.Properties.Headers.Add(headerProp.Key, headerProp.Value);
                }
            }

            return messageBuilder;
        }

        /// <summary>
        /// Publishes message from clients into RabbitMQ.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        public void AcceptMessage(ServiceBusMessage message)
        {
            if (string.IsNullOrEmpty(message.ClientID))
            {
                throw new ArgumentNullException(nameof(message.ClientID));
            }

            if (string.IsNullOrEmpty(message.MessageTypeID))
            {
                throw new ArgumentNullException(nameof(message.MessageTypeID));
            }

            var password = ConfigurationManager.AppSettings["DefaultRmqUserPassword"];
            var connectionFactory = GetConnectionFactoryForUser(message.ClientID, password);

            try
            {
                using (var connection = connectionFactory.CreateConnection())
                {
                    var model = connection.CreateModel();

                    var exchange = _namingManager.GetExchangeName(message.MessageTypeID);
                    var routingKey = _namingManager.GetRoutingKey(message.MessageTypeID);

                    var messageBuilder = BuildMessage(message, model);

                    model.ConfirmSelect();
                    model.BasicPublish(exchange, routingKey, messageBuilder.Properties,
                        messageBuilder.GetContentBody());
                    //we should wait confirm from broker before accept message
                    model.WaitForConfirmsOrDie();
                }
            }
            catch (BrokerUnreachableException e)
            {
                _logger.LogUnhandledException(e,
                    title: $"Unavailable broker on publish message {message.MessageTypeID} from client {message.ClientID} to broker");
                throw;
            }
            catch (AlreadyClosedException e)
            {
                _logger.LogUnhandledException(e,
                    title: $"Broker rejected incoming message {message.MessageTypeID} from {message.ClientID}. Reason: {e.ShutdownReason}.");
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e, title: $"Error on receive message {message.MessageTypeID} from {message.ClientID}.");
            }
        }

        /// <summary>
        /// Publishes message with group into broker.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <param name="groupName">Group identifier.</param>
        public void AcceptMessage(ServiceBusMessage message, string groupName)
        {
            // TODO: реализовать
            throw new NotImplementedException();
        }

        /// <summary>
        /// Publishes message with empty payload into broker.
        /// </summary>
        /// <param name="clientId">Cliend Id.</param>
        /// <param name="eventTypeId">Event type Id.</param>
        public void RaiseEvent(string clientId, string eventTypeId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(eventTypeId))
            {
                throw new ArgumentNullException(nameof(eventTypeId));
            }

            var password = ConfigurationManager.AppSettings["DefaultRmqUserPassword"];

            var connectionFactory = GetConnectionFactoryForUser(clientId, password);
            try
            {
                using (var connection = connectionFactory.CreateConnection())
                {
                    var model = connection.CreateModel();

                    var exchange = _namingManager.GetExchangeName(eventTypeId);
                    var routingKey = _namingManager.GetRoutingKey(eventTypeId);

                    model.ConfirmSelect();
                    model.BasicPublish(exchange, routingKey);
                    // TODO: здесь нужно ловить исключение ошибки publish
                    model.WaitForConfirmsOrDie();
                }
            }
            catch (BrokerUnreachableException e)
            {
                _logger.LogUnhandledException(e,
                    title: $"Unavailable broker on publish event {eventTypeId} from client {clientId} to broker");
                throw;
            }
            catch (AlreadyClosedException e)
            {
                _logger.LogUnhandledException(e,
                    title: $"Broker rejected incoming event {eventTypeId} from {clientId}. Reason: {e.ShutdownReason}.");
            }
            catch (Exception e)
            {
                _logger.LogUnhandledException(e, title: $"Error on receive event {eventTypeId} from {clientId}.");
            }

        }
    }
}
