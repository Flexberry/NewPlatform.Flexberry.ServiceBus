namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Interface for manage messages stored in RabbitMQ.
    /// </summary>
    public interface IMessageManager
    {
        /// <summary>
        /// Returns count messages from RabbitMQ.
        /// </summary>
        /// <param name="clientId">Filter queues by client ID, empty string - do not filter.</param>
        /// <param name="messageTypeId">Filter queues by message type ID, empty string - do not filter.</param>
        /// <returns>Count messages from RabbitMQ.</returns>
        int CountMessages(string clientId, string messageTypeId);

        /// <summary>
        /// Returns messages from RabbitMQ.
        /// </summary>
        /// <param name="offset">Offset from start.</param>
        /// <param name="count">Count of messages.</param>
        /// <param name="clientId">Filter queues by client ID, empty string - do not filter.</param>
        /// <param name="messageTypeId">Filter queues by message type ID, empty string - do not filter.</param>
        /// <returns>Messages from RabbitMQ.</returns>
        Message[] GetMessages(int offset, int count, string clientId, string messageTypeId);
    }
}
