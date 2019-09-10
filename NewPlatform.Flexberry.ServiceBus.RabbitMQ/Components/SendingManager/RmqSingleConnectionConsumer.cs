namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using RabbitMQ.Client;

    /// <summary>
    /// RabbitMQ consumer using single connection from constructor.
    /// </summary>
    internal class RmqSingleConnectionConsumer : BaseRmqConsumer
    {
        private readonly IConnection _connection;

        protected override IConnection Connection => _connection;

        /// <inheritdoc />
        public RmqSingleConnectionConsumer(ILogger logger, IMessageConverter converter, IConnection connection,
            Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders) : base(logger, converter, subscription, defaultPrefetchCount, useLegacySenders)
        {
            _connection = connection;
            connection.ConnectionShutdown -= OnConnectionShutdown;
            connection.ConnectionShutdown += OnConnectionShutdown;

            connection.RecoverySucceeded -= OnRecoverySucceeded;
            connection.RecoverySucceeded += OnRecoverySucceeded;

            connection.ConnectionRecoveryError -= OnConnectionRecoveryError;
            connection.ConnectionRecoveryError += OnConnectionRecoveryError;
        }
    }
}