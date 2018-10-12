namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// В данном классе шина выступает в роли клиента, принимая и отправляя сообщения.
    /// </summary>
    public partial class SBService : IServiceBusClient
    {
        /// <summary>
        /// Отправить сообщение в шину.
        /// </summary>
        /// <param name="message">Структура данных, описывающая отправляемое сообщение.</param>
        void IServiceBusClient.SendMessage(ServiceBusMessage message)
        {
            _receivingManager.AcceptMessage(message);
        }

        /// <summary>
        /// Отправить сообщение в шину с заданным именем группы.
        /// </summary>
        /// <param name="message">Структура данных, описывающая отправляемое сообщение.</param>
        /// <param name="group">Имя группы.</param>
        void IServiceBusClient.SendMessageWithGroup(ServiceBusMessage message, string group)
        {
            _receivingManager.AcceptMessage(message, group);
        }

        /// <summary>
        /// Метод получения сообщения из шины клиентом.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения, которое нужно получить.</param>
        /// <returns>Найденное сообщение, либо <c>null</c>.</returns>
        ServiceBusMessage IServiceBusClient.GetMessage(string clientId, string messageTypeId)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId);
            ServiceBusMessage result = null;
            if (message != null)
            {
                result = ServiceHelper.CreateWcfMessageFromEsb(
                    message.ReceivingTime,
                    message.MessageType.ID,
                    message.Body,
                    message.Sender,
                    message.Group,
                    ServiceHelper.GetTagDictionary(message),
                    message.BinaryAttachment);

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));

                _sendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            }

            return result;
        }

        /// <summary>
        /// Метод получения сообщения с указанным именем группы из шины клиентом.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения, которое нужно получить.</param>
        /// <param name="group">Имя группы запрашиваемого сообщения.</param>
        /// <returns>Найденное сообщение, либо <c>null</c>.</returns>
        ServiceBusMessage IServiceBusClient.GetMessageWithGroup(string clientId, string messageTypeId, string group)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId, group);
            ServiceBusMessage result = null;
            if (message != null)
            {
                result = ServiceHelper.CreateWcfMessageFromEsb(
                    message.ReceivingTime,
                    message.MessageType.ID,
                    message.Body,
                    message.Sender,
                    message.Group,
                    ServiceHelper.GetTagDictionary(message),
                    message.BinaryAttachment);

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));

                _sendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            }

            return result;
        }

        /// <summary>
        /// Получить сообщение из шины с соответствующими тэгами.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Тип запрашиваемого сообщения.</param>
        /// <param name="tags">Тэги, которые должно содержать сообщение.</param>
        /// <returns>Найденное сообщение или <c>null</c>.</returns>
        ServiceBusMessage IServiceBusClient.GetMessageWithTags(string clientId, string messageTypeId, string[] tags)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId, tags);
            ServiceBusMessage result = null;
            if (message != null)
            {
                result = ServiceHelper.CreateWcfMessageFromEsb(
                    message.ReceivingTime,
                    message.MessageType.ID,
                    message.Body,
                    message.Sender,
                    message.Group,
                    ServiceHelper.GetTagDictionary(message),
                    message.BinaryAttachment);

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));

                _sendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            }

            return result;
        }

        /// <summary>
        /// Получить информацию о сообщении.
        /// </summary>
        /// <param name="clientId">Строковый идентификатор клиента.</param>
        /// <param name="messageTypeId">Строковый идентификатор типа сообщения.</param>
        /// <returns>Информация о приоритете и времени формирования сообщения. Если ни одного сообщения не найдено, то <c>null</c>.</returns>
        ServiceBusMessageInfo IServiceBusClient.GetMessageInfo(string clientId, string messageTypeId)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, 1).FirstOrDefault();
            return info == null ? null : new ServiceBusMessageInfo { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Получить информацию о сообщении с заданной группой.
        /// </summary>
        /// <param name="clientId">Строковой идентификатор клиента.</param>
        /// <param name="messageTypeId">Строковой идентификатор типа сообщения.</param>
        /// <param name="group">Имя группы.</param>
        /// <returns>Информация о приоритете и времени формирования сообщения. Если ни одного сообщения не найдено, то <c>null</c>.</returns>
        ServiceBusMessageInfo IServiceBusClient.GetMessageInfoWithGroup(string clientId, string messageTypeId, string group)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, group, 1).FirstOrDefault();
            return info == null ? null : new ServiceBusMessageInfo { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Получить информацию о сообщении с заданными тегами.
        /// </summary>
        /// <param name="clientId">Строковой идентификатор клиента.</param>
        /// <param name="messageTypeId">Строковой идентификатор типа сообщения.</param>
        /// <param name="tags">Теги.</param>
        /// <returns>Информация о приоритете и времени формирования сообщения. Если ни одного сообщения не найдено, то <c>null</c>.</returns>
        ServiceBusMessageInfo IServiceBusClient.GetMessageInfoWithTags(string clientId, string messageTypeId, string[] tags)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, tags, 1).FirstOrDefault();
            return info == null ? null : new ServiceBusMessageInfo { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Получить текущее количество неотправленных сообщений указанного типа для указанного клиента.
        /// </summary>
        /// <param name="clientId">
        /// Идентификатор клиента.
        /// </param>
        /// <param name="messageTypeId">
        /// Идентификатор типа сообщения.
        /// </param>
        /// <returns>
        /// Количество сообщений.
        /// </returns>
        int IServiceBusClient.GetCurrentMessageCountByMessageType(string clientId, string messageTypeId)
        {
            return _sendingManager.GetCurrentMessageCount(clientId, messageTypeId);
        }

        /// <summary>
        /// Получить текущее количество неотправленных сообщений для указанного клиента.
        /// </summary>
        /// <param name="clientId">
        /// Идентификатор клиента.
        /// </param>
        /// <returns>
        /// Количество сообщений.
        /// </returns>
        int IServiceBusClient.GetCurrentMessageCount(string clientId)
        {
            return _sendingManager.GetCurrentMessageCount(clientId);
        }
    }
}
