namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Threading.Tasks;
    using NewPlatform.Flexberry.ServiceBus.MessageSenders;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    /// <summary>
    /// Base RabbitMQ consumer.
    /// </summary>
    public abstract class BaseRmqConsumer : AsyncDefaultBasicConsumer
    {
        /// <summary>
        /// Logger component.
        /// </summary>
        protected readonly ILogger Logger;

        private IMessageSender sender;
        private readonly IMessageConverter converter;
        private readonly ushort defaultPrefetchCount;
        private AmqpNamingManager namingManager = new AmqpNamingManager();
        private bool useLegacySenders;
        private ushort prefetchCount;

        /// <summary>
        /// Get alive connection to RabbitMQ.
        /// </summary>
        protected abstract IConnection Connection { get; }

        private ushort GetPrefetchCount(Subscription subscription)
        {
            if (subscription.Client.ConnectionsLimit.HasValue && subscription.Client.ConnectionsLimit > 0)
            {
                return (ushort)Math.Min(subscription.Client.ConnectionsLimit.Value, ushort.MaxValue);
            }
            else
            {
                return defaultPrefetchCount;
            }
        }

        private bool CheckShouldRecreate(ShutdownEventArgs reason)
        {
            return AlwaysRecreate ||
                   reason.ReplyCode == 530 || // attempt to reuse consumer tag
                   reason.ReplyCode == 0 || // internal library error
                   reason.ReplyCode == 541; // unexpected exception in library
        }

        /// <summary>
        /// Get consumer tag, should be unique.
        /// </summary>
        /// <returns>Consumer tag.</returns>
        protected string GetConsumerTag()
        {
            string queueName = namingManager.GetClientQueueName(this.Subscription.Client.ID, this.Subscription.MessageType.ID);
            return $"{queueName}_{Guid.NewGuid().ToString("N")}";
        }

        /// <summary>
        /// Handle connection recovery error.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="reason">Event args.</param>
        protected void OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs reason)
        {
            this.Logger.LogError("Callback sender event", $"Connection's recovery of {this.ConsumerTag} failed. {Environment.NewLine} {reason.Exception}");
        }

        /// <summary>
        /// Handle consumer recovery.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="reason">Event args.</param>
        protected void OnRecoverySucceeded(object sender, EventArgs reason)
        {
            this.Logger.LogInformation("Callback sender event", $"Connection of {this.ConsumerTag} is recovered.");
        }

        /// <summary>
        /// Handle connection shutdown.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="reason">Event args.</param>
        protected void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            this.Logger.LogInformation("Callback sender event", $"Connection of {this.ConsumerTag} shutdown. Reason: {reason.ToString()}");
            ShouldRecreate = CheckShouldRecreate(reason);
        }

        /// <summary>
        /// Handle model shutdown.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="reason">Event args.</param>
        protected void OnModelShutdown(object sender, ShutdownEventArgs reason)
        {
            this.Logger.LogInformation("Callback sender event", $"Model of {this.ConsumerTag} shutdown. Reason: {reason.ToString()}");
            ShouldRecreate = CheckShouldRecreate(reason);
        }

        /// <summary>
        /// Handle model recovery.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="reason">Event args.</param>
        protected void ModelOnBasicRecoverOk(object sender, EventArgs e)
        {
            this.Logger.LogInformation("Callback sender event", $"Model {this.ConsumerTag} is recovered.");
        }

        /// <summary>
        /// Create RabbitMQ consumer with self creating connection.
        /// </summary>
        /// <param name="logger">Logger component.</param>
        /// <param name="converter">RabbitMQ message to flexberry message converter.</param>
        /// <param name="subscription">Subscription for consumer.</param>
        /// <param name="defaultPrefetchCount">Default prefetch count.</param>
        /// <param name="useLegacySenders">Use legacy senders.</param>
        protected BaseRmqConsumer(ILogger logger, IMessageConverter converter, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.defaultPrefetchCount = defaultPrefetchCount;
            this.useLegacySenders = useLegacySenders;
            this.Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
            this.sender = new MessageSenderCreator(logger, useLegacySenders).GetMessageSender(subscription);
            this.prefetchCount = GetPrefetchCount(subscription);
        }

        /// <summary>
        /// Flag about consumer should restart in all fail cases.
        /// </summary>
        public bool AlwaysRecreate { get; set; }

        /// <summary>
        /// Flag about consumer should restart due to changing prefetch count or inability to recover.
        /// </summary>
        public bool ShouldRecreate { get; private set; }

        /// <summary>
        /// Subscription using for consumer creating.
        /// </summary>
        public Subscription Subscription { get; private set; }

        /// <summary>
        /// Update subscription data (sending type, address, client's connection limit).
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        public void UpdateSubscription(Subscription subscription)
        {
            if (subscription.TransportType != this.Subscription.TransportType ||
                subscription.Client.Address != this.Subscription.Client.Address)
            {
                this.sender = new MessageSenderCreator(this.Logger, useLegacySenders).GetMessageSender(subscription);
                this.Subscription = subscription;
            }

            uint subPrefetchCount = GetPrefetchCount(subscription);

            if (prefetchCount != subPrefetchCount)
            {
                ShouldRecreate = true;
            }
        }

        /// <summary>
        /// Start consumer.
        /// </summary>
        public void Start()
        {
            string queueName = this.namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID);

            try
            {
                if (this.Model != null)
                {
                    this.Model.Dispose();
                }

                this.Model = Connection.CreateModel();
                this.Model.ConfirmSelect();
                this.Model.BasicQos(0, this.prefetchCount, false);
                this.Model.BasicConsume(this, queueName, false, GetConsumerTag());

                this.Model.ModelShutdown -= OnModelShutdown;
                this.Model.ModelShutdown += OnModelShutdown;

                this.Model.BasicRecoverOk -= ModelOnBasicRecoverOk;
                this.Model.BasicRecoverOk += ModelOnBasicRecoverOk;

                this.Logger.LogDebugMessage("", $"Created listener of queue {queueName}");
            }
            catch (Exception ex)
            {
                this.Logger.LogInformation($"Can't create listener of queue {queueName}", ex.ToString());
                ShouldRecreate = true;
            }
        }
        
        /// <summary>
        /// Stop consumer and dispose resourses.
        /// </summary>
        public virtual void Stop()
        {
            this.Logger.LogDebugMessage("",
                $"Stopped listener of queue {this.namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
            this.Model?.Dispose();
        }

        /// <summary>
        /// Send message from RabbitMQ to callback.
        /// </summary>
        /// <param name="consumerTag">Consumer tag.</param>
        /// <param name="deliveryTag">Message delivery tag.</param>
        /// <param name="redelivered">Is message redelivered.</param>
        /// <param name="exchange">Original exchange.</param>
        /// <param name="routingKey">Routing key.</param>
        /// <param name="properties">Message properties.</param>
        /// <param name="body">Message content.</param>
        /// <returns></returns>
        public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            this.Logger.LogDebugMessage($"Callback sender event",
                $"Received message from queue {this.namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");

            MessageWithNotTypedPk message = this.converter.ConvertFromMqFormat(body, properties.Headers);
            message.SendingTime = DateTime.Now;
            message.MessageType = Subscription.MessageType;
            message.Recipient = this.Subscription.Client;

            // TODO: вынести логику в отдельный компонент?
            // TODO: Подумать о равномерной нагрузке клиентов
            bool sended = this.sender.SendMessage(message);
            if (sended)
            {
                this.Model.BasicAck(deliveryTag, false);
                this.Logger.LogDebugMessage($"Callback sender event",
                    $"Acked message from queue {this.namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
            }
            else
            {
                this.Model.BasicReject(deliveryTag, false);
            }
        }

        public override bool Equals(object obj)
        {
            var otherConsumer = obj as BaseRmqConsumer;

            if (otherConsumer == null)
            {
                return false;
            }

            return this.Subscription.Client.ID == otherConsumer.Subscription.Client.ID &&
                   this.Subscription.MessageType.ID == otherConsumer.Subscription.MessageType.ID;
        }

        public override int GetHashCode()
        {
            return (this.Subscription.Client.ID + this.Subscription.MessageType.ID).GetHashCode();
        }
    }
}