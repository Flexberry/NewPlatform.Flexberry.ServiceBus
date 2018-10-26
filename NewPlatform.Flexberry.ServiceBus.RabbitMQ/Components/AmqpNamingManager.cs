namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;

    /// <summary>
    /// Класс для перевода наименований объектов маршрутизации шины и наименований объектов в AMQP.
    /// </summary>
    internal class AmqpNamingManager
    {
        public string ExchangePrefix => "ics-messageTypeRouting_";

        public string ClientQueuePrefix => "ics-consumer_";

        private char _queueClientTypeDelimiter = '@';

        /// <summary>
        /// Получить имя точки обмена по типу сообщения в шине.
        /// </summary>
        /// <param name="messageTypeId">Тип сообщения в шине.</param>
        /// <returns>Имя точки обмена в AMQP.</returns>
        public string GetExchangeName(string messageTypeId)
        {
            return $"{this.ExchangePrefix}{messageTypeId}";
        }

        /// <summary>
        /// Получить название очереди для клиента по типу сообщения.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Тип сообщения.</param>
        /// <returns>Имя очереди.</returns>
        public string GetClientQueueName(string clientId, string messageTypeId)
        {
            return $"{this.GetClientQueuePrefix(clientId)}{this._queueClientTypeDelimiter}{messageTypeId}";
        }

        /// <summary>
        /// Получить начало наименования очереди клиента.
        /// </summary>
        /// <param name="clienId">Идентификатор клиента.</param>
        /// <returns>Начало наименования очереди клиента.</returns>
        public string GetClientQueuePrefix(string clienId)
        {
            return $"{this.ClientQueuePrefix}{clienId}";
        }

        /// <summary>
        /// Получить ключ маршрутизации для типа сообщения.
        /// </summary>
        /// <param name="messageTypeId">Тип сообщения.</param>
        /// <returns>Ключ маршутизации.</returns>
        public string GetRoutingKey(string messageTypeId)
        {
            return messageTypeId;
        }

        /// <summary>
        /// Получить ключ маршрутизации для типа сообщения.
        /// </summary>
        /// <param name="messageTypeId">Тип сообщения.</param>
        /// <returns>Ключ маршутизации.</returns>
        public string GetMessageType(string routingKey)
        {
            return routingKey;
        }

        /// <summary>
        /// Получить подписку шины по точке обмена, названию очереди и ключа маршрутизации.
        /// </summary>
        /// <param name="exchangeName">Название точки обмена.</param>
        /// <param name="queueName">Название очереди.</param>
        /// <param name="routingKey">Ключ маршрутизации.</param>
        /// <returns>Объект подписки с заполенным MessageType.ID и Client.ID.</returns>
        /// TODO: возможно следует выкинуть метод и пользоваться атрибутами очереди (проверить есть ли атрибуты очереди в AMQP/JMS или это специфика RMQ)
        public Subscription GetSubscriptionByAmqpModel(string queueName, string routingKey)
        {
            if (!queueName.StartsWith(this.ClientQueuePrefix) && queueName.Contains(this._queueClientTypeDelimiter.ToString()))
            {
                throw new Exception($"Не совместимая очередь {queueName}");
            }

            return new Subscription()
            {
                Client = new Client()
                {
                    ID = queueName.Replace(this.ClientQueuePrefix, string.Empty)
                                     .Replace(routingKey, string.Empty)
                                     .Replace(this._queueClientTypeDelimiter.ToString(), string.Empty)
                },
                MessageType = new MessageType()
                {
                    ID = routingKey
                }
            };
        }

        /// <summary>
        /// Returns client ID (0) and message type ID (1) from queue name.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <returns>Client ID and message type ID.</returns>
        public string[] GetIDsFromQueueName(string queueName)
        {
            return queueName.Replace(ClientQueuePrefix, string.Empty).Split(_queueClientTypeDelimiter);
        }
    }
}
