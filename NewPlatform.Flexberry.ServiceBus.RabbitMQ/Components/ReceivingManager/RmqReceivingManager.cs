using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSSoft.Services;
using Microsoft.Practices.Unity.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using Unity;
using Unity.Resolution;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Модуль приёма сообщений в формате шины в брокер
    /// </summary>
    internal class RmqReceivingManager : BaseServiceBusComponent, IReceivingManager
    {
        private IMessageConverter _messageConverter;

        private AmqpNamingManager _namingManager;

        public readonly string ConnectionFactoryRegistrationName = "RmqReceivingManagerConnFactory";

        /// <summary>
        /// Получение фабрики подключений для указанного пользователя.
        /// </summary>
        /// <param name="username">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>Фабрика подключений с проставленным username и password</returns>
        protected IConnectionFactory GetConnectionFactoryForUser(string username, string password)
        {
            // вот здесь важно создавать новый контейнер, а не брать существующий
            var container = new UnityContainer().LoadConfiguration();
            //иначе здесь мы можем изменить IConnectionFactory, который используется в других классах
            var connectionFactory = container.Resolve<IConnectionFactory>();
            connectionFactory.UserName = username;
            connectionFactory.Password = password;

            return connectionFactory;
        }

        public RmqReceivingManager(IMessageConverter converter)
        {
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
