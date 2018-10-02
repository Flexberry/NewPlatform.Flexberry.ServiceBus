using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Content;
namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Модуль приёма сообщений в формате шины в брокер
    /// </summary>
    internal class RmqReceivingManager : BaseReceivingManager
    {
        private IConnectionFactory _connectionFactory;

        private IMessageConverter _messageConverter;

        private AmqpNamingManager _namingManager;

        public RmqReceivingManager(IConnectionFactory connectionFactory, IMessageConverter converter)
        {
            _connectionFactory = connectionFactory;
            _messageConverter = converter;
            _namingManager = new AmqpNamingManager();
        }

        /// <summary>
        /// Собирает map-message для публикации в брокере.
        /// </summary>
        /// <param name="message">Сообщение в формате шины.</param>
        /// <param name="model">AMQP-модель.</param>
        /// <returns>Сообщение со всеми заполненными полями.</returns>
        private MapMessageBuilder BuildMessage(MessageForESB message, IModel model)
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
        public override void AcceptMessage(MessageForESB message)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                var exchange = _namingManager.GetExchangeName(message.MessageTypeID);
                var routingKey = _namingManager.GetRoutingKey(message.MessageTypeID);

                var messageBuilder = BuildMessage(message, model);

                model.BasicPublish(exchange, routingKey, messageBuilder.Properties, messageBuilder.GetContentBody());
            }
        }

        /// <summary>
        /// Принять сообщение с указанной группой. Не реализовано.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        /// <param name="groupName">Имя группы.</param>
        public override void AcceptMessage(MessageForESB message, string groupName)
        {
            // TODO: реализовать
            throw new NotImplementedException();
        }

        /// <summary>
        /// Принять уведомление.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="eventTypeId">Идентификатор уведомления (события).</param>
        public override void RaiseEvent(string clientId, string eventTypeId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                var exchange = _namingManager.GetExchangeName(eventTypeId);
                var routingKey = _namingManager.GetRoutingKey(eventTypeId);

                model.BasicPublish(exchange, routingKey);
            }
        }
    }
}
