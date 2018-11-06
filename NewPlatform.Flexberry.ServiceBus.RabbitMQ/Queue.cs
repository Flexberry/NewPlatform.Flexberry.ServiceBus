namespace NewPlatform.Flexberry.ServiceBus
{
    using ICSSoft.STORMNET;

    /// <summary>
    /// Queue.
    /// </summary>
    [NotStored()]
    [View("ListView", new string[] { "Recipient as \'Получатель\'", "MessageType as \'Тип сообщения\'", "Messages as \'Количество сообщений\'" })]
    public class Queue : DataObject
    {
        /// <summary>
        /// Queue name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Recipient ID.
        /// </summary>
        public virtual string Recipient { get; set; }

        /// <summary>
        /// Message type ID.
        /// </summary>
        public virtual string MessageType { get; set; }

        /// <summary>
        /// The number of messages in the queue.
        /// </summary>
        public virtual int Messages { get; set; }

        /// <summary>
        /// The name of VHost.
        /// </summary>
        public virtual string VHost { get; set; }

        /// <summary>
        /// Class views container.
        /// </summary>
        public class Views
        {
            /// <summary>
            /// "ListView" view.
            /// </summary>
            public static View ListView => Information.GetView("ListView", typeof(Queue));
        }
    }
}
