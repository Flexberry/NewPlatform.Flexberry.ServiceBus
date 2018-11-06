namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

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
        private class RmqConsumer : DefaultBasicConsumer
        {
            private readonly ILogger _logger;

            /// <summary>
            /// Получание подписки слушателя.
            /// </summary>
            public Subscription Subscription { get; private set; }

            private IMessageSender _sender;
            private readonly IMessageConverter _converter;
            private readonly IConnectionFactory _connectionFactory;
            private IModel _model;
            private readonly AmqpNamingManager _namingManager = new AmqpNamingManager();

            /// <summary>
            /// Number of minutes to be added to delay before the next attempt to send message.
            /// </summary>
            public int AdditionalMinutesBetweenRetries { get; set; } = 3;

            public RmqConsumer(ILogger logger, IMessageConverter converter, IConnectionFactory connectionFactory, Subscription subscription)
            {
                _logger = logger;
                Subscription = subscription;
                _converter = converter;
                _connectionFactory = connectionFactory;
                _sender = new MessageSenderCreator(logger).GetMessageSender(subscription);
            }

            /// <summary>
            /// Обновление данных подписки (необходимо в случае если 
            /// </summary>
            /// <param name="subscription">Подписка</param>
            public void UpdateSubscription(Subscription subscription)
            {
                if (subscription.TransportType != this.Subscription.TransportType || subscription.Client.Address != this.Subscription.Client.Address)
                {
                    this._sender = new MessageSenderCreator(this._logger).GetMessageSender(subscription);
                    this.Subscription = subscription;
                }
            }

            public void Start()
            {
                var connection = _connectionFactory.CreateConnection();
                _model = connection.CreateModel();

                var queueName = this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID);
                this._logger.LogDebugMessage("", $"Создан слушатель очереди {queueName}");

                this._model.BasicConsume(queueName, false, this);
            }

            public void Stop()
            {
                this._model.Dispose();
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
            {
                var message = this._converter.ConvertFromMqFormat(body, properties.Headers);
                message.SendingTime = DateTime.Now;
                message.MessageType = Subscription.MessageType;

                try
                {
                    // TODO: вынести логику в отдельный компонент?
                    // TODO: Подумать о равномерной нагрузке клиентов
                    this._sender.SendMessage(message);
                }
                catch (Exception e)
                {
                    this._logger.LogError("Ошибка отправки сообщения", e.ToString());
                    if (redelivered)
                    {
                        Thread.Sleep(AdditionalMinutesBetweenRetries * 60 * 1000);
                    }

                    this.Model.BasicNack(deliveryTag, false, true);
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

        /// <summary>
        /// Частота запуска синхронизации подписок.
        /// </summary>
        public int UpdatePeriodMilliseconds { get; set; } = 30 * 1000;

        protected MessageSenderCreator MessageSenderCreator;
        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IManagementClient _managementClient;
        private readonly IMessageConverter _converter;
        private readonly AmqpNamingManager _namingManager;
        private readonly Vhost _vhost;

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


        public RmqSendingManager(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, IConnectionFactory connectionFactory, IManagementClient managementClient, IMessageConverter converter, AmqpNamingManager namingManager, string vhost = "/")
        {
            this._logger = logger;
            this._esbSubscriptionsManager = esbSubscriptionsManager;
            this._connectionFactory = connectionFactory;
            this._managementClient = managementClient;
            this._converter = converter;
            this._namingManager = namingManager;
            this.MessageSenderCreator = new MessageSenderCreator(_logger);
            this._vhost = this._managementClient.CreateVirtualHostAsync(vhost).Result;

            this._consumers = new List<RmqConsumer>();
        }

        public void Prepare()
        {
            var subscriptions = _esbSubscriptionsManager.GetCallbackSubscriptions();
            foreach (var subscription in subscriptions)
            {
                this._consumers.Add(new RmqConsumer(_logger, _converter, _connectionFactory, subscription));
            }

            this._actualizationTimer = new Timer(x => this.Actualize(), null, this.UpdatePeriodMilliseconds, this.UpdatePeriodMilliseconds);
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
            var allConsumers = subscriptions.Select(x => new RmqConsumer(_logger, _converter, _connectionFactory, x)).ToList();

            // Множество новых слушатей = Текущие подписки - запущенные подписки
            var newConsumers = allConsumers.Except(this._consumers);

            // Множество слушателей на удаление = Запущенные подписки - текущие подписки
            var consumersToDelete = this._consumers.Except(allConsumers);

            foreach (var newConsumer in newConsumers)
            {
                newConsumer.Start();
            }

            foreach (var consumerToDelete in consumersToDelete)
            {
                consumerToDelete.Stop();
            }

            // Обновляем данные подписки (на случай если изменился тип callback'а или адрес)
            foreach (var _consumer in this._consumers)
            {
                var actualConsumer = allConsumers.First(x => x.Equals(_consumer));
                _consumer.UpdateSubscription(actualConsumer.Subscription);
            }
        }

        public void Start()
        {
            foreach (var consumer in this._consumers)
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
            return this._managementClient.GetQueueAsync(queueName, this._vhost).Result.Messages;
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
