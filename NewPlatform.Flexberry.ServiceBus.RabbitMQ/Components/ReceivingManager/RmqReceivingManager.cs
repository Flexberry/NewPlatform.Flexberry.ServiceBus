namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Content;
    using RabbitMQ.Client.Framing;
    using System;
    using System.Configuration;

    /// <summary>
    /// Component for receiving ESB-model messages from clients to RabbitMQ.
    /// </summary>
    internal class RmqReceivingManager : BaseServiceBusComponent, IReceivingManager
    {
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

        public RmqReceivingManager(IMessageConverter converter, Uri rmqUri)
        {
            this.RmqUri = rmqUri;
            _messageConverter = converter;
            _namingManager = new AmqpNamingManager();
        }

        /// <summary>
        /// Собирает map-message для публикации в брокере.
        /// </summary>
        /// <param name="message">Сообщение в формате шины.</param>
        /// <param name="model">AMQP-модель.</param>
        /// <returns>Сообщение со всеми заполненными полями.</returns>
        private MapMessageBuilder BuildMessage(ServiceBusMessage message, IModel model)
        {
            var messageBuilder = new MapMessageBuilder(model);

            var bodyProps = _messageConverter.GetBodyProperties(message);
            foreach (var bodyProp in bodyProps)
            {
                messageBuilder.Body.Add(bodyProp.Key, bodyProp.Value);
            }

            var headerProps = _messageConverter.GetProperties(message);
            foreach (var headerProp in headerProps)
            {
                messageBuilder.Properties.Headers.Add(headerProp.Key, headerProp.Value);
            }

            return messageBuilder;
        }

        /// <summary>
        /// Приём сообщения в брокер.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
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
            // TODO: здесь нужно ловить исключение ошибки авторизации
            using (var connection = connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                var exchange = _namingManager.GetExchangeName(message.MessageTypeID);
                var routingKey = _namingManager.GetRoutingKey(message.MessageTypeID);

                var messageBuilder = BuildMessage(message, model);

                // чтобы быть уверенным, что сообщение попало в брокер, включаем режим подтверждений
                model.ConfirmSelect();
                model.BasicPublish(exchange, routingKey, messageBuilder.Properties, messageBuilder.GetContentBody());
                // TODO: здесь нужно ловить исключение ошибки publish
                model.WaitForConfirmsOrDie();
            }
        }

        /// <summary>
        /// Принять сообщение с указанной группой. Не реализовано.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        /// <param name="groupName">Имя группы.</param>
        public void AcceptMessage(ServiceBusMessage message, string groupName)
        {
            // TODO: реализовать
            throw new NotImplementedException();
        }

        /// <summary>
        /// Принять уведомление.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="eventTypeId">Идентификатор уведомления (события).</param>
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
            // TODO: здесь нужно ловить исключение ошибки авторизации
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
    }
}
