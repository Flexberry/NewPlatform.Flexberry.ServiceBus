namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;

    using RabbitMQ.Client;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    using Queue = EasyNetQ.Management.Client.Model.Queue;
    using Message = Message;

    /// <summary>
    /// Класс для доставки сообщений из RabbitMQ.
    /// </summary>
    internal class RmqSendingManager : ISendingManager
    {
        private class RmqConsumer : AsyncDefaultBasicConsumer
        {
            private readonly ILogger _logger;

            private IMessageSender _sender;
            private readonly IMessageConverter _converter;
            private readonly IConnectionFactory _connectionFactory;
            private readonly AmqpNamingManager _namingManager = new AmqpNamingManager();
            private readonly bool useLegacySenders;
            private readonly ushort _prefetchCount;

            /// <summary>
            /// Получание подписки слушателя.
            /// </summary>
            public Subscription Subscription { get; private set; }

            /// <summary>
            /// Number of minutes to be added to delay before the next attempt to send message.
            /// </summary>
            public int AdditionalMinutesBetweenRetries { get; set; } = 3;

            public RmqConsumer(ILogger logger, IMessageConverter converter, IConnectionFactory connectionFactory, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders)
            {
                _logger = logger;
                Subscription = subscription;
                _converter = converter;
                _connectionFactory = connectionFactory;
                this.useLegacySenders = useLegacySenders;
                _sender = new MessageSenderCreator(logger, useLegacySenders).GetMessageSender(subscription);

                if (Subscription.Client.ConnectionsLimit.HasValue && Subscription.Client.ConnectionsLimit > 0)
                {
                    this._prefetchCount = (ushort)Math.Min(Subscription.Client.ConnectionsLimit.Value, ushort.MaxValue);
                }
                else
                {
                    this._prefetchCount = defaultPrefetchCount;
                }
            }

            /// <summary>
            /// Обновление данных подписки (необходимо в случае если 
            /// </summary>
            /// <param name="subscription">Подписка</param>
            public void UpdateSubscription(Subscription subscription)
            {
                if (subscription.TransportType != this.Subscription.TransportType || subscription.Client.Address != this.Subscription.Client.Address)
                {
                    this._sender = new MessageSenderCreator(this._logger, useLegacySenders).GetMessageSender(subscription);
                    this.Subscription = subscription;
                }
            }

            public void Start()
            {
                var queueName =
                    this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID);

                try
                {
                    using (var connection = _connectionFactory.CreateConnection())
                    {
                        this.Model = connection.CreateModel();
                        this.Model.BasicQos(0, this._prefetchCount, false);
                        this.Model.BasicConsume(queueName, false, this);
                    }
                }
                catch(Exception ex)
                {
                    this._logger.LogInformation($"Can't create listener of queue {queueName}", ex.ToString());
                    this.IsRunning = false;
                    return;
                }

                this._logger.LogDebugMessage("", $"Created listener of queue {queueName}");
            }

            public void Stop()
            {
                this.Model.Dispose();
            }

            private string DeclareDelayRoutes(IModel model)
            {
                var sub = this.Subscription;

                var delayExchangeName = _namingManager.GetClientDelayExchangeName(sub.Client.ID);
                var delayQueueName = _namingManager.GetClientDelayQueueName(sub.Client.ID, sub.MessageType.ID);
                var delayRoutingKey = _namingManager.GetDelayRoutingKey(sub.Client.ID, sub.MessageType.ID);
                var originalQueueName = _namingManager.GetClientQueueName(sub.Client.ID, sub.MessageType.ID);
                var originalRoutingKey = _namingManager.GetRoutingKey(sub.MessageType.ID);

                var queueArguments = new Dictionary<string, object>();
                queueArguments["x-dead-letter-exchange"] = delayExchangeName;
                queueArguments["x-dead-letter-routing-key"] = originalRoutingKey;
                queueArguments[RabbitMqConstants.FlexberryArgumentsKeys.NotSyncFlag] = "";
                model.QueueDeclare(delayQueueName, true, false, false, queueArguments);
                model.ExchangeDeclare(delayExchangeName, RabbitMQ.Client.ExchangeType.Direct, true);
                model.QueueBind(delayQueueName, delayExchangeName, delayRoutingKey);
                model.QueueBind(originalQueueName, delayExchangeName, originalRoutingKey);

                return delayRoutingKey;
            }

            private void DelayMessage(ulong deliveryTag, IBasicProperties properties, byte[] body)
            {
                var connection = _connectionFactory.CreateConnection();
                var model = connection.CreateModel();
                model.ConfirmSelect();

                if (properties.Headers == null)
                    properties.Headers = new Dictionary<string, object>();

                long redeliveryCount = _converter.GetErrorsCount(properties.Headers);
                long delay = redeliveryCount * AdditionalMinutesBetweenRetries * 60 * 1000; // delay in ms
                properties.Expiration = delay.ToString();
                properties.Headers[RabbitMqConstants.FlexberryHeadersKeys.OriginalMessageTimestamp] = properties.Timestamp;

                var requeue = false;
                try
                {
                    var delayRoutingKey = DeclareDelayRoutes(model);
                    model.BasicPublish("", delayRoutingKey, false, properties, body);
                    model.WaitForConfirmsOrDie();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error on message delay", ex.ToString());
                    requeue = true;
                }

                this.Model.BasicReject(deliveryTag, requeue);
            }

            public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
            {
                _logger.LogDebugMessage($"Callback sender event", $"Received message from queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");

                var message = this._converter.ConvertFromMqFormat(body, properties.Headers);
                message.SendingTime = DateTime.Now;
                message.MessageType = Subscription.MessageType;
                message.Recipient = this.Subscription.Client;

                // TODO: вынести логику в отдельный компонент?
                // TODO: Подумать о равномерной нагрузке клиентов
                var sended = this._sender.SendMessage(message);
                if(sended)
                {

                    this.Model.BasicAck(deliveryTag, false);
                    _logger.LogDebugMessage($"Callback sender event", $"Acked message from queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
                }
                else
                {
                    DelayMessage(deliveryTag, properties, body);
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(RmqConsumer))
                {
                    return false;
                }

                var otherConsumer = (RmqConsumer) obj;

                return this.Subscription.Client.ID == otherConsumer.Subscription.Client.ID &&
                       this.Subscription.MessageType.ID == otherConsumer.Subscription.MessageType.ID;
            }

            public override int GetHashCode()
            {
                return (this.Subscription.Client.ID + this.Subscription.MessageType.ID).GetHashCode();
            }
        }

        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IManagementClient _managementClient;
        private readonly IMessageConverter _converter;
        private readonly AmqpNamingManager _namingManager;
        private readonly string _vhostName;
        private readonly bool useLegacySenders;
        private Vhost _vhost;
        private List<RmqConsumer> _consumers;
        private Timer _actualizationTimer;
        private IModel _sharedModel;

        private IModel SharedModel
        {
            get
            {
                if (_sharedModel == null || _sharedModel.IsClosed)
                {
                    var connection = _connectionFactory.CreateConnection();
                    _sharedModel = connection.CreateModel();
                }

                return _sharedModel;
            }
        }

        /// <summary>
        /// Задача по актуализации
        /// </summary>
        private void Actualize()
        {
            // Убираем из текущих слушателей остановленные
            var stoppedConsumers = this._consumers.Where(x => !x.IsRunning);
            this._consumers.RemoveAll(x => stoppedConsumers.Contains(x));

            var subscriptions = _esbSubscriptionsManager.GetCallbackSubscriptions();
            var allConsumers = subscriptions.Select(x => new RmqConsumer(_logger, _converter, _connectionFactory, x, DefaultPrefetchCount, useLegacySenders)).ToList();

            // Множество новых слушатей = Текущие подписки - запущенные подписки
            var newConsumers = allConsumers.Except(this._consumers);

            // Множество слушателей на удаление = Запущенные подписки - текущие подписки
            var consumersToDelete = this._consumers.Except(allConsumers);

            foreach (var newConsumer in newConsumers)
            {
                this._consumers.Add(newConsumer);
                newConsumer.Start();
            }

            foreach (var consumerToDelete in consumersToDelete)
            {
                consumerToDelete.Stop();
            }

            // Обновляем данные подписки (на случай если изменился тип callback'а или адрес)
            foreach (var consumer in this._consumers)
            {
                var actualConsumer = allConsumers.First(x => x.Equals(consumer));
                consumer.UpdateSubscription(actualConsumer.Subscription);
            }
        }

        protected MessageSenderCreator MessageSenderCreator;

        /// <summary>
        /// Частота запуска синхронизации подписок.
        /// </summary>
        public int UpdatePeriodMilliseconds { get; set; } = 30 * 1000;

        /// <summary>
        /// Max count of unacknowledged messages for per consumer.
        /// </summary>
        public ushort DefaultPrefetchCount { get; set; } = 10;

        /// <summary>
        /// Gets Vhost RabbitMq.
        /// </summary>
        public Vhost Vhost
        {
            get
            {
                if (_vhost == null)
                {
                    _vhost = this._managementClient.CreateVirtualHostAsync(_vhostName).Result;
                }
                return _vhost;
            }
        }

        public RmqSendingManager(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, IConnectionFactory connectionFactory, IManagementClient managementClient, IMessageConverter converter, AmqpNamingManager namingManager, string vhost = "/", bool useLegacySenders = true)
        {
            this._logger = logger;
            this._esbSubscriptionsManager = esbSubscriptionsManager;
            this._connectionFactory = connectionFactory;
            this._managementClient = managementClient;
            this._converter = converter;
            this._namingManager = namingManager;
            this.MessageSenderCreator = new MessageSenderCreator(_logger, useLegacySenders);
            this._vhostName = vhost;
            this.useLegacySenders = useLegacySenders;

            this._consumers = new List<RmqConsumer>();
        }

        public void Prepare()
        {
            var subscriptions = _esbSubscriptionsManager.GetCallbackSubscriptions();
            foreach (var subscription in subscriptions)
            {
                this._consumers.Add(new RmqConsumer(_logger, _converter, _connectionFactory, subscription, DefaultPrefetchCount, useLegacySenders));
            }

            this._actualizationTimer = new Timer(x => this.Actualize(), null, this.UpdatePeriodMilliseconds, this.UpdatePeriodMilliseconds);
        }

        public void Start()
        {
            var consumers = this._consumers.ToArray();

            foreach (var consumer in consumers)
            {
                try
                {
                    consumer.Start();
                }
                catch (Exception e)
                {
                    _logger.LogError("Ошибка запуска слушателя брокера", e.ToString());
                }
            }
        }

        public void Stop()
        {
            this._consumers.ForEach(x => x.Stop());
        }

        public void AfterStop()
        {
        }

        public void QueueForSending(Message msg)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Получение количества сообщений для клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Количество сообщений в очередях клиента.</returns>
        public int GetCurrentMessageCount(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("", nameof(clientId));
            }

            // Получаем очереди клиента.
            var queuePrefix = _namingManager.GetClientQueuePrefix(clientId);
            var queues = _managementClient.GetQueuesAsync().Result.Where(x => x.Name.StartsWith(queuePrefix)).ToList();

            // Суммируем количество сообщений в них.
            return queues.Sum(x => x.Messages);
        }

        /// <summary>
        /// Получение количества сообщений для подписчика.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <returns>Количество сообщений в очереди подписчика.</returns>
        public int GetCurrentMessageCount(string clientId, string messageTypeId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("", nameof(clientId));
            }

            if (string.IsNullOrEmpty(messageTypeId))
            {
                throw new ArgumentException("", nameof(messageTypeId));
            }

            var queueName = this._namingManager.GetClientQueueName(clientId, messageTypeId);
            return this._managementClient.GetQueueAsync(queueName, this.Vhost).Result.Messages;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public IEnumerable<ServiceBusMessageInfo> GetMessagesInfo(string clientId, int maxCount = 0)
        {
            string queueNamePrefix = _namingManager.GetClientQueuePrefix(clientId);
            IEnumerable<Queue> queues = _managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueNamePrefix));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = _managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                    {
                        // TODO: Добавить заполнение других свойств.
                        MessageTypeID = _namingManager.GetMessageType(message.RoutingKey),
                    };

                    rmqMessagesInfo.Add(msg);
                }

                if (maxCount > 0 && rmqMessagesInfo.Count >= maxCount)
                {
                    rmqMessagesInfo = rmqMessagesInfo.Take(maxCount).ToList();
                    break;
                }
            }

            return rmqMessagesInfo;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщений.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public IEnumerable<ServiceBusMessageInfo> GetMessagesInfo(string clientId, string messageTypeId, int maxCount = 0)
        {
            var queueName = _namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = _managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = _managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                    {
                        // TODO: Добавить заполнение других свойств.
                        MessageTypeID = messageTypeId,
                    };

                    rmqMessagesInfo.Add(msg);
                }

                if (maxCount > 0 && rmqMessagesInfo.Count >= maxCount)
                {
                    rmqMessagesInfo = rmqMessagesInfo.Take(maxCount).ToList();
                    break;
                }
            }

            return rmqMessagesInfo;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="groupName">Имя группы сообщения.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public IEnumerable<ServiceBusMessageInfo> GetMessagesInfo(string clientId, string messageTypeId, string groupName, int maxCount = 0)
        {
            var queueName = _namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = _managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = _managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    // TODO: Исправить считывание групп.
                    if (message.Properties.Headers.Where(pr => pr.Key.StartsWith("__gruop" + groupName)).ToArray().Length != 0)
                    {
                        ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                        {
                            // TODO: Добавить заполнение других свойств.
                            MessageTypeID = _namingManager.GetMessageType(message.RoutingKey),
                        };

                        rmqMessagesInfo.Add(msg);
                    }
                }

                if (maxCount > 0 && rmqMessagesInfo.Count >= maxCount)
                {
                    rmqMessagesInfo = rmqMessagesInfo.Take(maxCount).ToList();
                    break;
                }
            }

            return rmqMessagesInfo;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщений.</param>
        /// <param name="tags">Теги, которые должно содержать сообщение.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public IEnumerable<ServiceBusMessageInfo> GetMessagesInfo(string clientId, string messageTypeId, string[] tags, int maxCount = 0)
        {
            var queueName = _namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = _managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = _managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    var haveAllTags = true;
                    foreach (string tag in tags)
                    {
                        var headerTag = message.Properties.Headers.Where(pr => pr.Key.StartsWith(_converter.GetTagPropertiesPrefix(tag)));
                        if (headerTag.ToArray().Length == 0)
                        {
                            haveAllTags = false;
                            break;
                        }
                    }

                    if (haveAllTags)
                    {
                        ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                        {
                            // TODO: Добавить заполнение других свойств.
                            MessageTypeID = _namingManager.GetMessageType(message.RoutingKey),
                        };

                        rmqMessagesInfo.Add(msg);
                    }
                }

                if (maxCount > 0 && rmqMessagesInfo.Count >= maxCount)
                {
                    rmqMessagesInfo = rmqMessagesInfo.Take(maxCount).ToList();
                    break;
                }
            }

            return rmqMessagesInfo;
        }

        /// <summary>
        /// Метод не реализован.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>Сообщение с указанным идентификатором.</returns>
        public Message ReadMessage(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Получение сообщения для подписчика.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <returns>Сообщения или null, если сообщения нет.</returns>
        public Message ReadMessage(string clientId, string messageTypeId)
        {
            MessageWithNotTypedPk result = null;

            var queueName = _namingManager.GetClientQueueName(clientId, messageTypeId);
            var message = SharedModel.BasicGet(queueName, false);
            if (message != null)
            {
                result = _converter.ConvertFromMqFormat(message.Body, message.BasicProperties.Headers);
                result.MessageType = new MessageType()
                {
                    ID = messageTypeId
                };
                result.__PrimaryKey = message.DeliveryTag;
            }

            return result;
        }

        /// <summary>
        /// Не реализован.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор сообщения.</param>
        /// <param name="index">Индекс сообщения в очереди.</param>
        /// <returns>Сообщение из брокера.</returns>
        public Message ReadMessage(string clientId, string messageTypeId, int index)
        {
            throw new System.NotImplementedException();
        }

        public Message ReadMessage(string clientId, string messageTypeId, string groupName)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Получение сообщения подписчика с фильтрацией по тэгам.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="tags">Тэги.</param>
        /// <returns>Сообщение для клиента.</returns>
        public Message ReadMessage(string clientId, string messageTypeId, string[] tags)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Проверить наличие в очереди сообщения события.
        /// </summary>
        /// <param name="clientId">Идентификатор подписчика события.</param>
        /// <param name="eventTypeId">Идентификатор события.</param>
        /// <returns>Признак наступления события (наличие сообщения в очереди).</returns>
        public bool CheckEventIsRaised(string clientId, string eventTypeId)
        {
            return this.ReadMessage(clientId, eventTypeId) != null;
        }

        /// <summary>
        /// Подтверждение получения сообщения с заданным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор сообщения.</param>
        /// <returns>Признак успешеного подтверждения.</returns>
        public bool DeleteMessage(string id)
        {
            var rmqId = ulong.Parse(id);
            SharedModel.BasicAck(rmqId, false);

            // похоже нет способа понять есть ли сообщение с заданным ID, поэтому только так
            return true;
        }
    }
}
