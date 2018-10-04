using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Management.Client.Model;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EasyNetQ.Management.Client;
    using NewPlatform.Flexberry.ServiceBus.MessageSenders;
    using RabbitMQ.Client;

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

        public MessageInfoFromESB[] GetMessagesInfo(string clientId, int maxCount = 0)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
        }

        public MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, int maxCount = 0)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
        }

        public MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, string groupName, int maxCount = 0)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
        }

        public MessageInfoFromESB[] GetMessagesInfo(string clientId, string messageTypeId, string[] tags, int maxCount = 0)
        {
            // TODO: реализовать
            throw new System.NotImplementedException();
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
            Message result = null;

            var queueName = _namingManager.GetClientQueueName(clientId, messageTypeId);
            using (var connection = _connectionFactory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var message = model.BasicGet(queueName, false);
                if (message != null)
                {
                    result = _converter.ConvertFromMqFormat(message.Body, message.BasicProperties.Headers);
                    result.MessageType = new MessageType()
                    {
                        ID = messageTypeId
                    };
                    result.__PrimaryKey = message.DeliveryTag;
                }
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
            using (var connection = _connectionFactory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var rmqId = ulong.Parse(id);
                model.BasicAck(rmqId, false);
            }

            // похоже нет способа понять есть ли сообщение с заданным ID, поэтому только так
            return true;
        }
    }
}
