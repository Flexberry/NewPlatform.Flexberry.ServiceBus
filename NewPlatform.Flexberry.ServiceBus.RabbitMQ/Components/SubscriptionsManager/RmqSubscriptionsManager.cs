namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;
    using ExchangeType = RabbitMQ.Client.ExchangeType;

    /// <summary>
    /// Класс работы с объектами маршрутизации RMQ.
    /// </summary>
    internal class RmqSubscriptionsManager : BaseServiceBusComponent, ISubscriptionsManager
    {
        private readonly ILogger _logger;
        private readonly IManagementClient _managementClient;
        private readonly AmqpNamingManager _namingManager;

        private readonly Vhost _vhost;

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="RmqSubscriptionManager"/> class.
        /// </summary>
        /// <param name="logger">Используемый компонент логирования.</param>
        /// <param name="managementClient">Фабрика соединений RabbitMQ.</param>
        /// <param name="vhost">Virtual host RabbitMQ</param>
        public RmqSubscriptionsManager(ILogger logger, IManagementClient managementClient, string vhost = "/")
        {
            this._logger = logger;
            this._managementClient = managementClient;
            this._vhost = this._managementClient.CreateVirtualHostAsync(vhost).Result;

            // TODO: следует ли выносить это в зависимости?
            this._namingManager = new AmqpNamingManager();
        }

        /// <summary>
        /// Создание пользователя в RabbitMQ.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="name">Имя клиента.</param>
        /// <param name="address">Адрес клиента.</param>
        public void CreateClient(string clientId, string name, string address = null)
        {
            try
            {
                // Если клиента с указанным ID нет в RabbitMQ, то бросается исключение.
                User client = _managementClient.GetUserAsync(clientId).Result;
            }
            catch(AggregateException ex)
            {
                if (ex.InnerException.GetType() == typeof(UnexpectedHttpStatusCodeException) && ((UnexpectedHttpStatusCodeException)ex.InnerException).StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    this._managementClient.CreateUserAsync(new UserInfo(clientId, ConfigurationManager.AppSettings["DefaultRmqUserPassword"])).Wait();
                }
                else
                {
                    throw ex;
                }
                
            }
        }

        /// <summary>
        /// Удаление очередей клиента в RabbitMQ.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        public void DeleteClient(string clientId)
        {
            var queueNamePrefix = this._namingManager.GetClientQueuePrefix(clientId);

            var queues = this._managementClient.GetQueuesAsync().Result;

            var queuesToDelete = queues.Where(x => x.Name.StartsWith(queueNamePrefix));
            foreach (var queue in queuesToDelete)
            {
                this._managementClient.DeleteQueueAsync(queue).Wait();
            }
        }

        /// <summary>
        /// Создание точки обмена по типу сообщения.
        /// </summary>
        /// <param name="msgTypeInfo">Структура, описывающая тип сообщения.</param>
        public void CreateMessageType(ServiceBusMessageType msgTypeInfo)
        {
            var exchangeName = this._namingManager.GetExchangeName(msgTypeInfo.Id);
            var exchangeInfo = new ExchangeInfo(exchangeName, ExchangeType.Topic, autoDelete: false, durable: true, @internal: false, arguments: null);

            this._managementClient.CreateExchangeAsync(exchangeInfo, _vhost).Wait();
        }

        /// <summary>
        /// Создание точки обмена для события.
        /// В RabbitMQ аналогично созданию обычного типа сообщения.
        /// </summary>
        /// <param name="eventTypeInfo">Структура, описывающая тип сообщения.</param>
        public void CreateEventType(ServiceBusMessageType eventTypeInfo)
        {
            this.CreateMessageType(eventTypeInfo);
        }

        /// <summary>
        /// Получение всех подписок в RabbitMQ.
        /// </summary>
        /// <param name="onlyActive">Параметр игнорируется.</param>
        /// <returns>Совместимые с шиной подписки RabbitMQ.</returns>
        public IEnumerable<Subscription> GetSubscriptions(bool onlyActive = true)
        {
            return this.GetAmqpSubscriptions();
        }

        /// <summary>
        /// Получение подписок клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="onlyActive">Параметр игнорируется.</param>
        /// <returns>Подписки клиента.</returns>
        public IEnumerable<Subscription> GetSubscriptions(string clientId, bool onlyActive = true)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            return this.GetAmqpSubscriptions(clientId);
        }

        /// <summary>
        /// Метод не реализован в связи с отсутствие понятия callback в RabbitMQ.
        /// </summary>
        /// <param name="onlyActive">Параметр игнорируется.</param>
        /// <exception cref="NotImplementedException">Метод не реализован.</exception>
        /// <returns>Метод гарантированно выкидывает исключение.</returns>
        public IEnumerable<Subscription> GetCallbackSubscriptions(bool onlyActive = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получение подписок для типа сообщения.
        /// Не возвращает подписки для <paramref name="senderId"/>.
        /// </summary>
        /// <param name="messageTypeId">Тип сообщения.</param>
        /// <param name="senderId">Идентификатор клиента, чьи подписки не будут возвращены.</param>
        /// <returns>Подписки с типом сообщения.</returns>
        public IEnumerable<Subscription> GetSubscriptionsForMsgType(string messageTypeId, string senderId = null)
        {
            var result = new List<Subscription>();

            var exchangeName = this._namingManager.GetExchangeName(messageTypeId);
            var exchange = this._managementClient.GetExchangeAsync(exchangeName, _vhost).Result;
            var bindings = this._managementClient.GetBindingsWithSourceAsync(exchange).Result;

            foreach (var binding in bindings)
            {
                if (binding.Destination.StartsWith(this._namingManager.ClientQueuePrefix))
                {
                    if (senderId != null &&
                        !binding.Destination.StartsWith(this._namingManager.GetClientQueuePrefix(senderId)))
                    {
                        result.Add(this._namingManager.GetSubscriptionByAmqpModel(binding.Destination, binding.RoutingKey));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Создаёт подписку в RMQ, если её нет.
        /// </summary>
        /// <param name="clientId">Идетификатор клиента (queue).</param>
        /// <param name="messageTypeId">Тип сообщения (routing key).</param>
        /// <param name="isCallback">Параметр игнорируется. Оставлен для совместимости с интерфейсом.</param>
        /// <param name="transportType">Параметр игнорируется. Оставлен для совместимости с интерфейсом.</param>
        /// <param name="expiryDate">Параметр игнорируется. Оставлен для совместимости с интерфейсом.</param>
        public void SubscribeOrUpdate(string clientId, string messageTypeId, bool isCallback, TransportType? transportType, DateTime? expiryDate = null)
        {
            var queueName = this._namingManager.GetClientQueueName(clientId, messageTypeId);
            var exchangeName = this._namingManager.GetExchangeName(messageTypeId);
            var routingKey = this._namingManager.GetRoutingKey(messageTypeId);

            var queue = this._managementClient.CreateQueueAsync(new QueueInfo(queueName, false, true, new InputArguments()), this._vhost).Result;
            var exchange = this._managementClient.CreateExchangeAsync(new ExchangeInfo(exchangeName, ExchangeType.Topic, false, true, false, new Arguments()), _vhost).Result;
            this._managementClient.CreateBinding(exchange, queue, new BindingInfo(routingKey)).Wait();
        }

        /// <summary>
        /// Метод не используется.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        public void UpdateAllSubscriptions(string clientId)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Subscription> GetAmqpSubscriptions(string clientId = null)
        {
            var result = new List<Subscription>();

            var prefix = string.IsNullOrEmpty(clientId)
                ? this._namingManager.ClientQueuePrefix
                : this._namingManager.GetClientQueuePrefix(clientId);

            var queues = this._managementClient.GetQueuesAsync().Result.Where(x => x.Name.StartsWith(prefix));
            foreach (var queue in queues)
            {
                var bindings = this._managementClient.GetBindingsForQueueAsync(queue).Result
                                    .Where(x => !string.IsNullOrEmpty(x.Source)); // откидываем стандартный binding, берем только те, что связывают с exchange
                foreach (var binding in bindings)
                {
                    result.Add(this._namingManager.GetSubscriptionByAmqpModel(queue.Name, binding.RoutingKey));
                }
            }

            return result;
        }

        public void UpdateClient(string clientId, ServiceBusClient client)
        {
            throw new NotImplementedException();
        }

        public void UpdateMessageType(string messageTypeId, ServiceBusMessageType messageType)
        {
            throw new NotImplementedException();
        }

        public void DeleteMessageType(string messageTypeId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubscription(string subscriptionId, ServiceBusSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public void DeleteSubscription(string subscriptionId)
        {
            throw new NotImplementedException();
        }
    }
}
