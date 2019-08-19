namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;


    internal class RmqConsumer : BaseRmqConsumer
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;

        public RmqConsumer(ILogger logger, IMessageConverter converter, IConnectionFactory connectionFactory, Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders) : base(logger, converter, subscription, defaultPrefetchCount, useLegacySenders)
        {
            _connectionFactory = connectionFactory;
        }

        protected override IConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.RecoverySucceeded += OnRecoverySucceeded;
                    _connection.ConnectionRecoveryError += OnConnectionRecoveryError;
                }
                else if(!_connection.IsOpen)
                {
                    _connection.Dispose();
                    _connection = _connectionFactory.CreateConnection();

                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.RecoverySucceeded += OnRecoverySucceeded;
                    _connection.ConnectionRecoveryError += OnConnectionRecoveryError;
                }

                return _connection;
            }
        }
    }
}
