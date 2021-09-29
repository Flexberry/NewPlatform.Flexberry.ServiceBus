namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    /// <summary>
    /// RabbitMQ consumer creating connection for self.
    /// </summary>
    internal class RmqConsumer : BaseRmqConsumer
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;

        /// <summary>
        /// Create RabbitMQ consumer with self creating connection.
        /// </summary>
        /// <param name="logger">Logger component.</param>
        /// <param name="converter">RabbitMQ message to flexberry message converter.</param>
        /// <param name="connectionFactory">RabbitMQ connection factory.</param>
        /// <param name="subscription">Subscription for consumer.</param>
        /// <param name="defaultPrefetchCount">Default prefetch count.</param>
        /// <param name="useLegacySenders">Use legacy senders.</param>
        public RmqConsumer(ILogger logger, IMessageConverter converter, IConnectionFactory connectionFactory, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders) : base(logger, converter, subscription, defaultPrefetchCount, useLegacySenders)
        {
            _connectionFactory = connectionFactory;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }

            base.Stop();
        }

        /// <summary>
        /// Get connection for RabbitMQ.
        /// </summary>
        protected override IConnection Connection
        {
            get
            {
                if (_connection == null || base.ShouldRecreate)
                {
                    _connection = _connectionFactory.CreateConnection();

                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.ConnectionShutdown += OnConnectionShutdown;

                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.ConnectionShutdown += OnConnectionShutdown;

                    _connection.RecoverySucceeded -= OnRecoverySucceeded;
                    _connection.RecoverySucceeded += OnRecoverySucceeded;

                    _connection.ConnectionRecoveryError -= OnConnectionRecoveryError;
                    _connection.ConnectionRecoveryError += OnConnectionRecoveryError;
                }

                return _connection;
            }
        }
    }
}
