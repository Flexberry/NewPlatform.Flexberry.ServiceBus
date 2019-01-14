namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;

    /// <summary>
    /// Класс для перевода наименований объектов маршрутизации шины и наименований объектов в AMQP.
    /// </summary>
    public class AmqpNamingManager
    {
        public string ExchangePrefix => "ics-messageTypeRouting_";

        public string ClientQueuePrefix => "ics-consumer_";

        public string DelaySuffix => "_delay";

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
        /// Get exchange's name for publishing messages with delay.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <returns></returns>
        public string GetClientDelayExchangeName(string clientId)
        {
            return GetExchangeName(clientId) + DelaySuffix;
        }

        /// <summary>
        /// Get queue's name for store message in delay.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="messageTypeId">Message type identifier.</param>
        /// <returns></returns>
        public string GetClientDelayQueueName(string clientId, string messageTypeId)
        {
            return GetClientQueueName(clientId, messageTypeId) + DelaySuffix;
        }

        /// <summary>
        /// Get routing key's name for publishing messages with delay.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="messageTypeId">Message type identifier.</param>
        /// <returns></returns>
        public string GetDelayRoutingKey(string clientId, string messageTypeId)
        {
            return GetClientQueueName(clientId, messageTypeId) + DelaySuffix;
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
        /// Extracts the client ID and message type ID from the queue name.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="clientId">Extracted from the queue name the client ID.</param>
        /// <param name="messageTypeId">Extracted from the queue name the message type ID.</param>
        public void ParseQueueName(string queueName, out string clientId, out string messageTypeId)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }
            else if (!queueName.StartsWith(ClientQueuePrefix) || !queueName.Contains(_queueClientTypeDelimiter.ToString()))
            {
                throw new ArgumentException(nameof(queueName));
            }

            if (queueName.EndsWith(DelaySuffix))
            {
                queueName = queueName.Remove(queueName.Length - DelaySuffix.Length);
            }

            string[] ids = queueName.Replace(ClientQueuePrefix, string.Empty).Split(_queueClientTypeDelimiter);
            clientId = ids[0];
            messageTypeId = ids[1];
        }
    }
}
