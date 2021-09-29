namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;
    using ExchangeType = RabbitMQ.Client.ExchangeType;
    using RabbitMQ.Client;

    /// <summary>
    /// Класс работы с объектами маршрутизации RMQ.
    /// </summary>
    internal class RmqSubscriptionsManager : BaseServiceBusComponent, ISubscriptionsManager
    {
        private readonly ILogger logger;
        private readonly IManagementClient managementClient;
        private readonly IConnectionFactory connectionFactory;
        private readonly AmqpNamingManager namingManager;
        private readonly string vhostStr;
        private Vhost vhost;

        private IModel modelField;

        private IModel model
        {
            get
            {
                if (modelField == null || modelField.IsClosed)
                {
                    var connection = connectionFactory.CreateConnection();
                    modelField = connection.CreateModel();
                }

                return modelField;
            }
        }

        /// <summary>
        /// Create routing for controlling delayed messages (messages have rejected).
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="messageTypeId">Message type ID</param>
        /// <param name="subQueue">RabbitMQ esb subscription queue name</param>
        private void DeclareDelayRoutes(string clientId, string messageTypeId, string subQueue)
        {
            string delayExchangeName = namingManager.GetClientDelayExchangeName(clientId);
            string delayQueueName = namingManager.GetClientDelayQueueName(clientId, messageTypeId);
            string delayRoutingKey = namingManager.GetDelayRoutingKey(clientId, messageTypeId);
            string originalRoutingKey = namingManager.GetRoutingKey(messageTypeId);

            // declare dead letter exhange and key for returning message to original queue
            var queueArguments = new Dictionary<string, object>();
            queueArguments["x-dead-letter-exchange"] = delayExchangeName;
            queueArguments["x-dead-letter-routing-key"] = originalRoutingKey;
            queueArguments[RabbitMqConstants.FlexberryArgumentsKeys.NotSyncFlag] = "";

            model.ExchangeDeclareNoWait(delayExchangeName, ExchangeType.Direct, true, false);
            model.QueueDeclareNoWait(delayQueueName, true, false, false, queueArguments);

            model.QueueBindNoWait(delayQueueName, delayExchangeName, delayRoutingKey, null);
            model.QueueBindNoWait(subQueue, delayExchangeName, originalRoutingKey, null);
        }

        /// <summary>
        /// Gets Vhost RabbitMq.
        /// </summary>
        public Vhost Vhost
        {
            get
            {
                if (vhost == null)
                {
                    vhost = this.managementClient.CreateVirtualHostAsync(vhostStr).Result;
                }
                return vhost;
            }
        }

        /// <summary>
        /// Creates new object of type <see cref="RmqSubscriptionManager"/> class.
        /// </summary>
        /// <param name="logger">Logging component.</param>
        /// <param name="managementClient">Management client for RabbitMQ.</param>
        /// <param name="connectionFactory">RabbitMQ connection factory.</param>
        /// <param name="vhost">Virtual host RabbitMQ.</param>
        public RmqSubscriptionsManager(ILogger logger, IManagementClient managementClient, IConnectionFactory connectionFactory, string vhost = "/")
        {
            this.logger = logger;
            this.managementClient = managementClient;
            this.connectionFactory = connectionFactory;
            this.vhostStr = vhost;

            // TODO: следует ли выносить это в зависимости?
            this.namingManager = new AmqpNamingManager();
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
                User client = managementClient.GetUserAsync(clientId).Result;
            }
            catch(AggregateException ex)
            {
                var unexpectedHttpStatusCodeException = ex.InnerException as UnexpectedHttpStatusCodeException;

                if (unexpectedHttpStatusCodeException != null && unexpectedHttpStatusCodeException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    this.managementClient.CreateUserAsync(new UserInfo(clientId, ConfigurationManager.AppSettings["DefaultRmqUserPassword"])).Wait();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Удаление очередей клиента в RabbitMQ.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        public void DeleteClient(string clientId)
        {
            string queueNamePrefix = this.namingManager.GetClientQueuePrefix(clientId);

            IEnumerable<Queue> queues = this.managementClient.GetQueuesAsync().Result;

            var queuesToDelete = queues.Where(x => x.Name.StartsWith(queueNamePrefix));
            foreach (var queue in queuesToDelete)
            {
                this.managementClient.DeleteQueueAsync(queue).Wait();
            }
        }

        /// <summary>
        /// Создание точки обмена по типу сообщения.
        /// </summary>
        /// <param name="msgTypeInfo">Структура, описывающая тип сообщения.</param>
        public void CreateMessageType(ServiceBusMessageType msgTypeInfo)
        {
            string exchangeName = this.namingManager.GetExchangeName(msgTypeInfo.ID);
            var exchangeInfo = new ExchangeInfo(exchangeName, ExchangeType.Topic, autoDelete: false, durable: true, @internal: false, arguments: null);

            this.managementClient.CreateExchangeAsync(exchangeInfo, vhost).Wait();
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

            string exchangeName = this.namingManager.GetExchangeName(messageTypeId);
            Exchange exchange = this.managementClient.GetExchangeAsync(exchangeName, Vhost).Result;
            IEnumerable<Binding> bindings = this.managementClient.GetBindingsWithSourceAsync(exchange).Result;

            foreach (var binding in bindings)
            {
                if (binding.Destination.StartsWith(this.namingManager.ClientQueuePrefix))
                {
                    if (senderId != null &&
                        !binding.Destination.StartsWith(this.namingManager.GetClientQueuePrefix(senderId)))
                    {
                        result.Add(this.namingManager.GetSubscriptionByAmqpModel(binding.Destination, binding.RoutingKey));
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
        /// <param name="subscribtionId">араметр игнорируется. Оставлен для совместимости с интерфейсом.</param>
        public void SubscribeOrUpdate(string clientId, string messageTypeId, bool isCallback, TransportType? transportType, DateTime? expiryDate = null, string subscribtionId = null)
        {
            string queueName = this.namingManager.GetClientQueueName(clientId, messageTypeId);
            string exchangeName = this.namingManager.GetExchangeName(messageTypeId);
            string routingKey = this.namingManager.GetRoutingKey(messageTypeId);

            var queueArguments = new Dictionary<string, object>();
            queueArguments["x-dead-letter-exchange"] = namingManager.GetClientDelayExchangeName(clientId);
            queueArguments["x-dead-letter-routing-key"] = namingManager.GetDelayRoutingKey(clientId, messageTypeId);

            model.QueueDeclareNoWait(queueName, true, false, false, queueArguments);
            model.ExchangeDeclareNoWait(exchangeName, ExchangeType.Topic, true);
            model.QueueBindNoWait(queueName, exchangeName, routingKey, null);

            DeclareDelayRoutes(clientId, messageTypeId, queueName);
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

            string prefix = string.IsNullOrEmpty(clientId)
                ? this.namingManager.ClientQueuePrefix
                : this.namingManager.GetClientQueuePrefix(clientId);

            var queues = this.managementClient.GetQueuesAsync().Result.Where(x =>
                x.Name.StartsWith(prefix) && !x.Arguments.ContainsKey(RabbitMqConstants.FlexberryArgumentsKeys.NotSyncFlag));
            foreach (var queue in queues)
            {
                var bindings = this.managementClient.GetBindingsForQueueAsync(queue).Result
                                    .Where(x => !string.IsNullOrEmpty(x.Source)); // откидываем стандартный binding, берем только те, что связывают с exchange
                foreach (var binding in bindings)
                {
                    result.Add(this.namingManager.GetSubscriptionByAmqpModel(queue.Name, binding.RoutingKey));
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
