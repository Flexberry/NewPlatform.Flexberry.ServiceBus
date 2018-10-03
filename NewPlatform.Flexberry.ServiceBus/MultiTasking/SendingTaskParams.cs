namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Структура для передачи необходимых параметров потоку отправки сообщения.
    /// </summary>
    public struct SendingTaskParams
    {
        /// <summary>
        /// Первичный ключ отправляемого в запускаемом потоке сообщения.
        /// </summary>
        public Guid MessagePk;

        /// <summary>
        /// Подписка, по которой отправляется сообщение.
        /// </summary>
        public Subscription Subscription;

        /// <summary>
        /// Промежуток времени для вычисления момента следующей отправки сообщения.
        /// </summary>
        public int AdditionalTimeout;

        /// <summary>
        /// Словарь отложенных сообщений, куда добавляется сообщение в случае неудачной отправки.
        /// </summary>
        public ConcurrentDictionary<Guid, DateTime> DeferredMessages;
    }
}
