using System;
using System.Collections.Generic;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using RabbitMQ.Client;
    using System.Linq;

    /// <summary>
    /// Connection factory for creating connection to one specified URI.
    /// </summary>
    public class MultihostConnectionFactory : ConnectionFactory
    {
        private AmqpTcpEndpoint[] _endpoints;

        /// <summary>
        ///  
        /// </summary>
        /// <param name="endpoints">AMQP URIs</param>
        public MultihostConnectionFactory(Uri[] endpoints) : this(endpoints.Select(x => new AmqpTcpEndpoint(x)).ToArray())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoints">AMQP URIs</param>
        public MultihostConnectionFactory(AmqpTcpEndpoint[] endpoints)
        {
            this._endpoints = endpoints;
        }

        /// <summary>
        /// Create connection to one of specified endpoint.
        /// </summary>
        /// <returns></returns>
        public override IConnection CreateConnection()
        {
            return base.CreateConnection(_endpoints);
        }
    }
}