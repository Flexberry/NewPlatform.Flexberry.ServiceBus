namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections;
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
    internal partial class RmqSendingManager : ISendingManager
    {
        private readonly ILogger logger;
        private readonly ISubscriptionsManager esbSubscriptionsManager;
        private readonly IConnectionFactory connectionFactory;
        private readonly IManagementClient managementClient;
        private readonly IMessageConverter converter;
        private readonly AmqpNamingManager namingManager;
        private readonly string vhostName;
        private readonly bool useLegacySenders;
        private Vhost vhost;
        private List<BaseRmqConsumer> consumers;
        private Timer actualizationTimer;
        private static readonly object ActualizeLock = new object();
        private IModel sharedModelField;
        private IConnection connectionField;

        private IConnection connection
        {
            get
            {
                if (UseSingleConnection)
                {
                    if (connectionField == null)
                    {
                        connectionField = connectionFactory.CreateConnection();
                        logger.LogDebugMessage("Consumer connection creation", "");
                    }

                    return connectionField;
                }
                else
                {
                    return connectionFactory.CreateConnection();
                }
            }
        }

        private IModel sharedModel
        {
            get
            {
                if (sharedModelField == null || sharedModelField.IsClosed)
                {
                    var connection = connectionFactory.CreateConnection();
                    sharedModelField = connection.CreateModel();
                }

                return sharedModelField;
            }
        }

        private BaseRmqConsumer CreateConsumer(Subscription subscription)
        {
            BaseRmqConsumer rmqConsumer;
            if (UseSingleConnection)
            {
                rmqConsumer = new RmqSingleConnectionConsumer(logger, converter, connection, subscription, DefaultPrefetchCount, useLegacySenders);
            }
            else
            {
                rmqConsumer = new RmqConsumer(logger, converter, connectionFactory, subscription, DefaultPrefetchCount, useLegacySenders);
            }

            rmqConsumer.AlwaysRecreate = AlwaysRecreateConsumer;

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
                var subscriptions = esbSubscriptionsManager.GetCallbackSubscriptions().ToArray();
                var aliveSubs = new List<BaseRmqConsumer>();

                foreach (var subscription in subscriptions)
                {
                    var subscriptionPk = subscription.__PrimaryKey;
                    BaseRmqConsumer rmqConsumer = this.consumers.FirstOrDefault(x => x.Subscription.__PrimaryKey.Equals(subscriptionPk));

                    if (rmqConsumer == null) // create if not exists
                    {
                        rmqConsumer = CreateConsumer(subscription);

                        try
                        {
                            rmqConsumer.Start();
                        }
                        catch (Exception e)
                        {
                            logger.LogError("Rmq consumer events", $"Error on starting consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
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
                                logger.LogError("Rmq consumer events", $"Error on updating consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
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
                                logger.LogError("Rmq consumer events", $"Error on stopping consumer {subscription.Client.ID}, message type {subscription.MessageType.ID}. {e.ToString()}");
                            }
                        }
                    }

                    aliveSubs.Add(rmqConsumer);
                }

                foreach (var rmqConsumer in this.consumers) // stop consumers of non-existings subscriptions
                {
                    if (!aliveSubs.Contains(rmqConsumer))
                    {
                        rmqConsumer.Stop();
                    }
                }

                this.consumers = aliveSubs;
            }
        }

        protected MessageSenderCreator MessageSenderCreator;

        /// <summary>
        /// Should use single connection for all consumers. 
        /// </summary>
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
                if (vhost == null)
                {
                    vhost = this.managementClient.CreateVirtualHostAsync(vhostName).Result;
                }
                return vhost;
            }
        }

        /// <summary>
        /// Should recreate RabbitMQ consumers on their fails.
        /// </summary>
        public bool AlwaysRecreateConsumer { get; set; } = false;

        public RmqSendingManager(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, IConnectionFactory connectionFactory, IManagementClient managementClient, IMessageConverter converter, AmqpNamingManager namingManager, string vhost = "/", bool useLegacySenders = true)
        {
            this.logger = logger;
            this.esbSubscriptionsManager = esbSubscriptionsManager;
            this.connectionFactory = connectionFactory;
            this.managementClient = managementClient;
            this.converter = converter;
            this.namingManager = namingManager;
            this.MessageSenderCreator = new MessageSenderCreator(this.logger, useLegacySenders);
            this.vhostName = vhost;
            this.useLegacySenders = useLegacySenders;

            this.consumers = new List<BaseRmqConsumer>();
        }

        /// <summary>
        /// Prepare component (prepare suscription data).
        /// </summary>
        public void Prepare()
        {
            var subscriptions = esbSubscriptionsManager.GetCallbackSubscriptions();
            foreach (var subscription in subscriptions)
            {
                this.consumers.Add(CreateConsumer(subscription));
            }
        }

        public void Start()
        {
            var consumers = this.consumers.ToArray();

            foreach (var consumer in consumers)
            {
                try
                {
                    consumer.Start();
                }
                catch (Exception e)
                {
                    logger.LogError("Ошибка запуска слушателя брокера", e.ToString());
                }
            }

            this.actualizationTimer = new Timer(x => this.Actualize(), null, this.UpdatePeriodMilliseconds, this.UpdatePeriodMilliseconds);
        }

        public void Stop()
        {
            foreach (var consumer in consumers)
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
            var queuePrefix = namingManager.GetClientQueuePrefix(clientId);
            var queues = managementClient.GetQueuesAsync().Result.Where(x => x.Name.StartsWith(queuePrefix)).ToList();

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

            string queueName = this.namingManager.GetClientQueueName(clientId, messageTypeId);
            return this.managementClient.GetQueueAsync(queueName, this.Vhost).Result.Messages;
        }

        /// <summary>
        /// Получить информацию о сообщениях, которые есть, но еще не отправлены указанному клиенту.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="maxCount">Максимальное количество возвращаемых записей. Если равно 0, возвращается информация о всех имеющихся сообщениях.</param>
        /// <returns>Информация о сообщениях. Записи отсортированы в планируемом порядке отправки.</returns>
        public IEnumerable<ServiceBusMessageInfo> GetMessagesInfo(string clientId, int maxCount = 0)
        {
            string queueNamePrefix = namingManager.GetClientQueuePrefix(clientId);
            IEnumerable<Queue> queues = managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueNamePrefix));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                    {
                        // TODO: Добавить заполнение других свойств.
                        MessageTypeID = namingManager.GetMessageType(message.RoutingKey),
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
            string queueName = namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
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
            string queueName = namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    // TODO: Исправить считывание групп.
                    if (message.Properties.Headers.Where(pr => pr.Key.StartsWith("__gruop" + groupName)).ToArray().Length != 0)
                    {
                        ServiceBusMessageInfo msg = new ServiceBusMessageInfo
                        {
                            // TODO: Добавить заполнение других свойств.
                            MessageTypeID = namingManager.GetMessageType(message.RoutingKey),
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
            string queueName = namingManager.GetClientQueueName(clientId, messageTypeId);
            IEnumerable<Queue> queues = managementClient.GetQueuesAsync().Result;
            IEnumerable<Queue> clientQueues = queues.Where(x => x.Name.StartsWith(queueName));

            List<ServiceBusMessageInfo> rmqMessagesInfo = new List<ServiceBusMessageInfo>();
            foreach (Queue clientQueue in clientQueues)
            {
                var messages = managementClient.GetMessagesFromQueueAsync(clientQueue, new GetMessagesCriteria(clientQueue.Messages, Ackmodes.ack_requeue_true)).Result;
                foreach (var message in messages)
                {
                    var haveAllTags = true;
                    foreach (string tag in tags)
                    {
                        var headerTag = message.Properties.Headers.Where(pr => pr.Key.StartsWith(converter.GetTagPropertiesPrefix(tag)));
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
                            MessageTypeID = namingManager.GetMessageType(message.RoutingKey),
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

            string queueName = namingManager.GetClientQueueName(clientId, messageTypeId);
            var message = sharedModel.BasicGet(queueName, false);
            if (message != null)
            {
                result = converter.ConvertFromMqFormat(message.Body, message.BasicProperties.Headers);
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
            sharedModel.BasicAck(rmqId, false);

            // похоже нет способа понять есть ли сообщение с заданным ID, поэтому только так
            return true;
        }
    }
}
