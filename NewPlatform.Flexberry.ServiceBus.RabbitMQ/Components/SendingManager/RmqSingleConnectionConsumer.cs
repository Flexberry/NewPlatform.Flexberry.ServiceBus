namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using RabbitMQ.Client;

    /// <summary>
    /// RabbitMQ consumer using single connection from constructor.
    /// </summary>
    internal class RmqSingleConnectionConsumer : BaseRmqConsumer
    {
        private readonly IConnection connection;

        protected override IConnection Connection => connection;

        /// <inheritdoc />
        public RmqSingleConnectionConsumer(ILogger logger, IMessageConverter converter, IConnection connection,
            Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders) : base(logger, converter, subscription, defaultPrefetchCount, useLegacySenders)
        {
            this.connection = connection;
            connection.ConnectionShutdown -= OnConnectionShutdown;
            connection.ConnectionShutdown += OnConnectionShutdown;

            connection.RecoverySucceeded -= OnRecoverySucceeded;
            connection.RecoverySucceeded += OnRecoverySucceeded;

            connection.ConnectionRecoveryError -= OnConnectionRecoveryError;
            connection.ConnectionRecoveryError += OnConnectionRecoveryError;
        }
    }
}