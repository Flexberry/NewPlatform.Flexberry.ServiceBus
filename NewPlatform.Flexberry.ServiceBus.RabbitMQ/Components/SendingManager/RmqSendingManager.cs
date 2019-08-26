namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Clustering;

    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;

    using RabbitMQ.Client;

    using MessageSenders;

    using Queue = EasyNetQ.Management.Client.Model.Queue;
    using Message = Message;

    /// <summary>
    /// Класс для доставки сообщений из RabbitMQ.
    /// </summary>
    internal partial class RmqSendingManager : ISendingManager, ISingletonClusterComponent
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IManagementClient _managementClient;
        private readonly IMessageConverter _converter;
        private readonly AmqpNamingManager _namingManager;
        private readonly string _vhostName;
        private readonly bool useLegacySenders;
        private Vhost _vhost;
        private List<BaseRmqConsumer> _consumers;
        private Timer _actualizationTimer;
        private static readonly object ActualizeLock = new object();
        private IModel _sharedModel;

        private IConnection _connection;

        private IConnection Connection
        {
            get
            {
                if (UseSingleConnection)
                {
                    if (_connection == null)
                    {
                        _connection = _connectionFactory.CreateConnection();
                        _logger.LogDebugMessage("Consumer connection creation", "");
                    }

                    return _connection;
                }
                else
                {
                    return _connectionFactory.CreateConnection();
                }
            }
        }

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


        public bool IsSingletonEnabled { get; set; } = false;

        private BaseRmqConsumer CreateConsumer(Subscription subscription)
        {
            BaseRmqConsumer rmqConsumer;
            if (UseSingleConnection)
            {
                rmqConsumer = new RmqSingleConnectionConsumer(_logger, _converter, Connection, subscription, DefaultPrefetchCount, useLegacySenders);
            }
            else
            {
                rmqConsumer = new RmqConsumer(_logger, _converter, _connectionFactory, subscription, DefaultPrefetchCount, useLegacySenders);
            }

            return rmqConsumer;
        }

        /// <summary>
        /// Задача по актуализации
        /// </summary>
        private void Actualize()
        {
            // on Mono 5.18 SynchronizedCollection did not work, will lock manually
            lock (ActualizeLock)
            {
                var subscriptions = _esbSubscriptionsManager.GetCallbackSubscriptions().ToArray();
                var aliveSubs = new List<BaseRmqConsumer>();

                foreach (var subscription in subscriptions)
                {
                    var subscriptionPk = subscription.__PrimaryKey;
                    var rmqConsumer = this._consumers.FirstOrDefault(x => x.Subscription.__PrimaryKey.Equals(subscriptionPk));

                    if (rmqConsumer == null) // create if not exists
                    {
                        rmqConsumer = CreateConsumer(subscription);

                        try
                        {
                            rmqConsumer.Start();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Rmq consumer events", $"Error on starting consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
                        }
                    }
                    else // actualize subscription data(transfer type and address)
                    {
                        if (rmqConsumer.IsRunning)
                        {
                            try
                            {
                                rmqConsumer.UpdateSubscription(subscription);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError("Rmq consumer events", $"Error on updating consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
                            }
                        }

                        if (rmqConsumer.ShouldRecreate)
                        {
                            try
                            {
                                rmqConsumer.Stop();
                                rmqConsumer = CreateConsumer(subscription);
                                rmqConsumer.Start();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError("Rmq consumer events", $"Error on stopping consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
                            }
                        }
                    }

                    aliveSubs.Add(rmqConsumer);
                }

                foreach (var rmqConsumer in this._consumers) // stop consumers of non-existings subscriptions
                {
                    if (!aliveSubs.Contains(rmqConsumer))
                    {
                        rmqConsumer.Stop();
                    }
                }

                this._consumers = aliveSubs;
            }
        }

        protected MessageSenderCreator MessageSenderCreator;

        public bool UseSingleConnection { get; set; } = false;

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

            this._consumers = new List<BaseRmqConsumer>();
        }

        public void Prepare()
        {
            var subscriptions = _esbSubscriptionsManager.GetCallbackSubscriptions();
            foreach (var subscription in subscriptions)
            {
                this._consumers.Add(CreateConsumer(subscription));
            }
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

            this._actualizationTimer = new Timer(x => this.Actualize(), null, this.UpdatePeriodMilliseconds, this.UpdatePeriodMilliseconds);
        }

        public void Stop()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Stop();
            }
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
