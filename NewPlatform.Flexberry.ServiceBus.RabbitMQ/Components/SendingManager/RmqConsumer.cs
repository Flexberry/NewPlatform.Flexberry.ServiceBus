namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using RabbitMQ.Client;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    internal class RmqConsumer : BaseRmqConsumer
    {
        private readonly IConnectionFactory _connectionFactory;

        protected override IConnection Connection => _connectionFactory.CreateConnection();

        public RmqConsumer(ILogger logger, IMessageConverter converter, IConnectionFactory connectionFactory,
            Subscription subscription, ushort defaultPrefetchCount, bool useLegacySenders) : base(logger, converter, subscription, defaultPrefetchCount, useLegacySenders)
        {
            _connectionFactory = connectionFactory;
        }
    }
}