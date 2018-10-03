namespace NewPlatform.Flexberry.ServiceBus.Components.Rerouter
{
    using System;

    /// <summary>
    /// Дополнительные данные о сообщении, которые передаются через шину.
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        /// ID контекста соединения.
        /// </summary>
        public Guid ContextId { get; set; }

        /// <summary>
        /// URL, на который требуется передать запрос.
        /// </summary>
        public string RerouteUrl { get; set; }

        /// <summary>
        /// ID типа сообщения шины, с которым требуется вернуть ответ.
        /// </summary>
        public string SbResponseType { get; set; }

        /// <summary>
        /// Тип содержимого.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Допустимые типы ответов.
        /// </summary>
        public string AcceptTypes { get; set; }

        /// <summary>
        /// Тип HTTP-запроса.
        /// </summary>
        public string HttpMethod { get; set; }
    }
}
