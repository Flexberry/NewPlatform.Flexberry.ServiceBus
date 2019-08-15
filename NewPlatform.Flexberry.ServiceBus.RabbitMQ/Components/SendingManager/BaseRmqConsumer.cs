using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using NewPlatform.Flexberry.ServiceBus.MessageSenders;
    using RabbitMQ.Client;

    public abstract class BaseRmqConsumer : AsyncDefaultBasicConsumer
    {
        protected readonly ILogger Logger;
        private IMessageSender _sender;
        private readonly IMessageConverter _converter;
        private AmqpNamingManager _namingManager = new AmqpNamingManager();
        private bool _useLegacySenders;
        private ushort _prefetchCount;

        protected abstract IConnection Connection { get; }

        protected BaseRmqConsumer(ILogger logger, IMessageConverter converter, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders)
        {
            Logger = logger;
            _converter = converter;
            _useLegacySenders = useLegacySenders;

            if (Subscription.Client.ConnectionsLimit.HasValue && Subscription.Client.ConnectionsLimit > 0)
            {
                _prefetchCount = (ushort)Math.Min(Subscription.Client.ConnectionsLimit.Value, ushort.MaxValue);
            }
            else
            {
                _prefetchCount = defaultPrefetchCount;
            }

            _sender = new MessageSenderCreator(logger, useLegacySenders).GetMessageSender(subscription);
        }

        /// <summary>
        /// Получание подписки слушателя.
        /// </summary>
        public Subscription Subscription { get; private set; }

        /// <summary>
        /// Number of minutes to be added to delay before the next attempt to send message.
        /// </summary>
        public int AdditionalMinutesBetweenRetries { get; set; } = 3;

        /// <summary>
        /// Обновление данных подписки (необходимо в случае если 
        /// </summary>
        /// <param name="subscription">Подписка</param>
        public void UpdateSubscription(Subscription subscription)
        {
            if (subscription.TransportType != this.Subscription.TransportType ||
                subscription.Client.Address != this.Subscription.Client.Address)
            {
                this._sender = new MessageSenderCreator(this.Logger, _useLegacySenders).GetMessageSender(subscription);
                this.Subscription = subscription;
            }
        }

        public void Start()
        {
            var queueName = this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID);

            try
            {
                this.Model = Connection.CreateModel();
                this.Model.BasicQos(0, this._prefetchCount, false);
                this.Model.BasicConsume(queueName, false, this);
            }
            catch (Exception ex)
            {
                this.Logger.LogInformation($"Can't create listener of queue {queueName}", ex.ToString());
                this.IsRunning = false;
                return;
            }

            this.Logger.LogDebugMessage("", $"Created listener of queue {queueName}");
        }

        public void Stop()
        {
            this.Logger.LogDebugMessage("",
                $"Stopped listener of queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
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
            var model = Connection.CreateModel();
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
                Logger.LogError("Error on message delay", ex.ToString());
                requeue = true;
            }

            this.Model.BasicReject(deliveryTag, requeue);
        }

        public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            Logger.LogDebugMessage($"Callback sender event",
                $"Received message from queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");

            var message = this._converter.ConvertFromMqFormat(body, properties.Headers);
            message.SendingTime = DateTime.Now;
            message.MessageType = Subscription.MessageType;
            message.Recipient = this.Subscription.Client;

            // TODO: вынести логику в отдельный компонент?
            // TODO: Подумать о равномерной нагрузке клиентов
            var sended = this._sender.SendMessage(message);
            if (sended)
            {

                this.Model.BasicAck(deliveryTag, false);
                Logger.LogDebugMessage($"Callback sender event",
                    $"Acked message from queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
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

            var otherConsumer = (RmqConsumer)obj;

            return this.Subscription.Client.ID == otherConsumer.Subscription.Client.ID &&
                   this.Subscription.MessageType.ID == otherConsumer.Subscription.MessageType.ID;
        }

        public override int GetHashCode()
        {
            return (this.Subscription.Client.ID + this.Subscription.MessageType.ID).GetHashCode();
        }
    }
}