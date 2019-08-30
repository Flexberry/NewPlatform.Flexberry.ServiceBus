using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using NewPlatform.Flexberry.ServiceBus.MessageSenders;
    using RabbitMQ.Client;

    /// <summary>
    /// Base RabbitMQ consumer.
    /// </summary>
    public abstract class BaseRmqConsumer : AsyncDefaultBasicConsumer
    {
        protected readonly ILogger Logger;
        private IMessageSender _sender;
        private readonly IMessageConverter _converter;
        private readonly ushort _defaultPrefetchCount;
        private AmqpNamingManager _namingManager = new AmqpNamingManager();
        private bool _useLegacySenders;
        private ushort _prefetchCount;

        protected abstract IConnection Connection { get; }

        private ushort GetPrefetchCount(Subscription subscription)
        {
            if (subscription.Client.ConnectionsLimit.HasValue && subscription.Client.ConnectionsLimit > 0)
            {
                return (ushort)Math.Min(subscription.Client.ConnectionsLimit.Value, ushort.MaxValue);
            }
            else
            {
                return _defaultPrefetchCount;
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
        /// Get consumertag, should be unique.
        /// </summary>
        /// <returns></returns>
        protected string GetConsumerTag()
        {
            var queueName = _namingManager.GetClientQueueName(this.Subscription.Client.ID, this.Subscription.MessageType.ID);
            return $"{queueName}_{Guid.NewGuid().ToString("N")}";
        }

        protected void OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs reason)
        {
            Logger.LogError("Callback sender event", $"Connection's recovery of {this.ConsumerTag} failed. {Environment.NewLine} {reason.Exception}");
        }

        protected void OnRecoverySucceeded(object sender, EventArgs reason)
        {
            Logger.LogInformation("Callback sender event", $"Connection of {this.ConsumerTag} is recovered.");
            ShouldRecreate = false;
        }

        protected void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            Logger.LogInformation("Callback sender event", $"Connection of {this.ConsumerTag} shutdown. Reason: {reason.ToString()}");
            ShouldRecreate = CheckShouldRecreate(reason);
        }

        protected void OnModelShutdown(object sender, ShutdownEventArgs reason)
        {
            Logger.LogInformation("Callback sender event", $"Model of {this.ConsumerTag} shutdown. Reason: {reason.ToString()}");
            ShouldRecreate = CheckShouldRecreate(reason);
        }

        protected void ModelOnBasicRecoverOk(object sender, EventArgs e)
        {
            Logger.LogInformation("Callback sender event", $"Model {this.ConsumerTag} is recovered.");
        }

        protected async Task OnConsumerCancelled(object sender, ConsumerEventArgs @event)
        {
            Logger.LogInformation("Consumer event", $"Consumer {this.ConsumerTag} cancelled. Event: {@event}");
        }

        protected BaseRmqConsumer(ILogger logger, IMessageConverter converter, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders)
        {
            Logger = logger;
            _converter = converter;
            _defaultPrefetchCount = defaultPrefetchCount;
            _useLegacySenders = useLegacySenders;
            Subscription = subscription;
            _sender = new MessageSenderCreator(logger, useLegacySenders).GetMessageSender(subscription);
            _prefetchCount = GetPrefetchCount(subscription);
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
        /// Получание подписки слушателя.
        /// </summary>
        public Subscription Subscription { get; private set; }

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

            var subPrefetchCount = GetPrefetchCount(subscription);

            if (_prefetchCount != subPrefetchCount)
            {
                ShouldRecreate = true;
            }
        }

        public void Start()
        {
            var queueName = this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID);

            try
            {
                Model?.Dispose();

                this.Model = Connection.CreateModel();
                this.Model.ConfirmSelect();
                this.Model.BasicQos(0, this._prefetchCount, false);
                this.Model.BasicConsume(this, queueName, false, GetConsumerTag());

                this.Model.ModelShutdown += OnModelShutdown;
                this.Model.BasicRecoverOk += ModelOnBasicRecoverOk;

                this.Logger.LogDebugMessage("", $"Created listener of queue {queueName}");
            }
            catch (Exception ex)
            {
                this.Logger.LogInformation($"Can't create listener of queue {queueName}", ex.ToString());
                ShouldRecreate = true;
            }
        }

        public virtual void Stop()
        {
            this.Logger.LogDebugMessage("",
                $"Stopped listener of queue {this._namingManager.GetClientQueueName(Subscription.Client.ID, Subscription.MessageType.ID)}");
            this.Model?.Dispose();
            this.ConsumerCancelled += OnConsumerCancelled;
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