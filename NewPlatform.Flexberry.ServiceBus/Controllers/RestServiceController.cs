namespace NewPlatform.Flexberry.ServiceBus.Controllers
{
    using System;
    using System.Configuration;
    using System.Web.Http;
    using Components;

    /// <summary>
    /// WebAPI контроллер для WebAPI сервиса шины.
    /// </summary>
    public class RestServiceController : ApiController
    {
        private readonly ISendingManager _sendingManager;

        private readonly IReceivingManager _receivingManager;

        public RestServiceController(ISendingManager sendingManager, IReceivingManager receivingManager)
        {
            if (sendingManager == null)
                throw new ArgumentNullException(nameof(sendingManager));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            _sendingManager = sendingManager;
            _receivingManager = receivingManager;
        }

        /// <summary>
        /// Получение списка сообщений для заданного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя сообщения.</param>
        /// <returns>Список сообщений, отсортированный по приоритету и времени формирования.</returns>
        [Route("Messages")]
        public virtual MessageInfoFromESB[] GetMessages(string clientId)
        {
            return _sendingManager.GetMessagesInfo(clientId);
        }

        /// <summary>
        /// Получить сообщение указанного типа для указанного получателя, соответствующее указанному индексу.
        /// </summary>
        /// <param name="clientId">Идентификатор получателя сообщения.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <param name="index">Индекс сообщения в отсортированном списке сообщений по приоритету и времени формирования.</param>
        /// <returns>Вычитанное сообщение, либо <c>null</c>, если сообщение не найдено для заданных аргументов.</returns>
        [Route("Message")]
        public virtual HttpMessageFromEsb GetMessage(string clientId, string messageTypeId, int index)
        {
            Message msg = _sendingManager.ReadMessage(clientId, messageTypeId, index);
            HttpMessageFromEsb result = null;
            if (msg != null)
            {
                result = ServiceHelper.CreateHttpMessageFromEsb(
                    msg.__PrimaryKey.ToString(),
                    msg.ReceivingTime,
                    msg.MessageType.ID,
                    msg.Body,
                    msg.Sender,
                    msg.Group,
                    ServiceHelper.GetTagDictionary(msg),
                    msg.BinaryAttachment);

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));
            }

            return result;
        }

        /// <summary>
        /// Получить сообщение по его идентификатору.
        /// </summary>
        /// <param name="id">Первичный ключ сообщения.</param>
        /// <returns>Вычитанное сообщение, либо <c>null</c>, если сообщения с таким ключом не существует.</returns>
        [Route("Message/{id}")]
        public virtual HttpMessageFromEsb GetMessage(string id)
        {
            Message msg = _sendingManager.ReadMessage(id);
            HttpMessageFromEsb result = null;
            if (msg != null)
            {
                result = ServiceHelper.CreateHttpMessageFromEsb(
                    msg.__PrimaryKey.ToString(),
                    msg.ReceivingTime,
                    msg.MessageType.ID,
                    msg.Body,
                    msg.Sender,
                    msg.Group,
                    ServiceHelper.GetTagDictionary(msg),
                    msg.BinaryAttachment);

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));
            }

            return result;
        }

        /// <summary>
        /// Отправить сообщение в шину.
        /// </summary>
        /// <param name="value">Структура данных, описывающая отправляемое сообщение.</param>
        [Route("Message")]
        [HttpPost]
        public virtual void PostMessage([FromBody] MessageForESB value)
        {
            _receivingManager.AcceptMessage(value);
        }

        /// <summary>
        /// Удалить сообщение из БД шины.
        /// </summary>
        /// <param name="id">Первичный ключ удаляемого сообщения.</param>
        [Route("Message/{id}")]
        [HttpDelete]
        public virtual void DeleteMessage(string id)
        {
            _sendingManager.DeleteMessage(id);
        }
    }
}