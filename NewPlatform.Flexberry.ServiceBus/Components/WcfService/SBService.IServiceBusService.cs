namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// Класс, обеспечивающий работу с шиной: прием сообщений, а также вычитка сообщений.
    /// </summary>
    public partial class SBService : IServiceBusService
    {
        /// <summary>
        /// Создать клиента в шине.
        /// </summary>
        /// <param name="clientId">Идентификатор создаваемого клиента.</param>
        /// <param name="name">Имя клиента.</param>
        /// <param name="address">Адрес для отправки сообщений клиенту.</param>
        public void CreateClient(string clientId, string name, string address)
        {
            _subscriptionsManager.CreateClient(clientId, name, address);
        }

        /// <summary>
        /// Удалить клиента.
        /// </summary>
        /// <param name="clientId">
        /// Идентификатор клиента, которого нужно удалить.
        /// </param>
        public void DeleteClient(string clientId)
        {
            _subscriptionsManager.DeleteClient(clientId);
        }

        /// <summary>
        /// Проверить существование уведомления о событии.
        /// </summary>
        /// <param name="clientId">Идентификатр клиента, для которого проверяется существование уведомления.</param>
        /// <param name="eventTypeId">Идентификатор клиента.</param>
        /// <returns><c>true</c>, если уведомление существует в БД, иначе <c>false</c>.</returns>
        public bool DoesEventRisen(string clientId, string eventTypeId)
        {
            Message message = _sendingManager.ReadMessage(clientId, eventTypeId);
            return message != null ? true : false;
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
        public int GetCurrentMessageCount(string clientId)
        {
            return _sendingManager.GetCurrentMessageCount(clientId);
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
        public int GetCurrentThisTypeMessageCount(string clientId, string messageTypeId)
        {
            return _sendingManager.GetCurrentMessageCount(clientId, messageTypeId);
        }

        /// <summary>
        /// Метод получения сообщения из шины клиентом.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения, которое нужно получить.</param>
        /// <returns>Найденное сообщение, либо <c>null</c>.</returns>
        public MessageFromESB GetMessageFromESB(string clientId, string messageTypeId)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId);
            MessageFromESB result = null;
            if (message != null)
            {
                result = new MessageFromESB
                {
                    MessageFormingTime = message.ReceivingTime,
                    MessageTypeID = message.MessageType.ID,
                    Body = message.Body,
                    Attachment = message.BinaryAttachment,
                    SenderName = message.Sender,
                    GroupID = message.Group,
                    Tags = ServiceHelper.GetTagDictionary(message),
                };

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
        public MessageOrderingInformation GetMessageInfo(string clientId, string messageTypeId)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, 1).FirstOrDefault();
            return info == null ? null : new MessageOrderingInformation { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Метод получения сообщения с указанным именем группы из шины клиентом.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения, которое нужно получить.</param>
        /// <param name="groupName">Имя группы запрашиваемого сообщения.</param>
        /// <returns>Найденное сообщение, либо <c>null</c>.</returns>
        public MessageFromESB GetMessageWithGroupFromESB(string clientId, string messageTypeId, string groupName)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId, groupName);
            MessageFromESB result = null;
            if (message != null)
            {
                result = new MessageFromESB
                {
                    MessageFormingTime = message.ReceivingTime,
                    MessageTypeID = message.MessageType.ID,
                    Body = message.Body,
                    Attachment = message.BinaryAttachment,
                    SenderName = message.Sender,
                    GroupID = message.Group,
                    Tags = ServiceHelper.GetTagDictionary(message),
                };

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));

                _sendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            }

            return result;
        }

        /// <summary>
        /// Получить информацию о сообщении с заданной группой.
        /// </summary>
        /// <param name="clientId">Строковой идентификатор клиента.</param>
        /// <param name="messageTypeId">Строковой идентификатор типа сообщения.</param>
        /// <param name="groupName">Имя группы.</param>
        /// <returns>Информация о приоритете и времени формирования сообщения. Если ни одного сообщения не найдено, то <c>null</c>.</returns>
        public MessageOrderingInformation GetMessageInfoWithGroup(string clientId, string messageTypeId, string groupName)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, groupName, 1).FirstOrDefault();
            return info == null ? null : new MessageOrderingInformation { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Получить сообщение из шины с соответствующими тэгами.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, запрашивающего сообщение.</param>
        /// <param name="messageTypeId">Тип запрашиваемого сообщения.</param>
        /// <param name="tags">Тэги, которые должно содержать сообщение.</param>
        /// <returns>Найденное сообщение или <c>null</c>.</returns>
        public MessageFromESB GetMessageWithTagsFromESB(string clientId, string messageTypeId, string[] tags)
        {
            Message message = _sendingManager.ReadMessage(clientId, messageTypeId, tags);
            MessageFromESB result = null;
            if (message != null)
            {
                result = new MessageFromESB
                {
                    MessageFormingTime = message.ReceivingTime,
                    MessageTypeID = message.MessageType.ID,
                    Body = message.Body,
                    Attachment = message.BinaryAttachment,
                    SenderName = message.Sender,
                    GroupID = message.Group,
                    Tags = ServiceHelper.GetTagDictionary(message),
                };

                if (result.Tags.ContainsKey("sendingWay"))
                    result.Tags["sendingWay"] += '/' + ConfigurationManager.AppSettings.Get("ServiceID4SB");
                else
                    result.Tags.Add("sendingWay", ConfigurationManager.AppSettings.Get("ServiceID4SB"));

                _sendingManager.DeleteMessage(message.__PrimaryKey.ToString());
            }

            return result;
        }

        /// <summary>
        /// Получить информацию о сообщении с заданными тегами.
        /// </summary>
        /// <param name="clientId">Строковой идентификатор клиента.</param>
        /// <param name="messageTypeId">Строковой идентификатор типа сообщения.</param>
        /// <param name="tags">Теги.</param>
        /// <returns>Информация о приоритете и времени формирования сообщения. Если ни одного сообщения не найдено, то <c>null</c>.</returns>
        public MessageOrderingInformation GetMessageInfoWithTags(string clientId, string messageTypeId, string[] tags)
        {
            ServiceBusMessageInfo info = _sendingManager.GetMessagesInfo(clientId, messageTypeId, tags, 1).FirstOrDefault();
            return info == null ? null : new MessageOrderingInformation { Priority = info.Priority, FormingTime = info.FormingTime };
        }

        /// <summary>
        /// Уведомить о событии.
        /// </summary>
        /// <param name="clientId">
        /// Идентификатор клиента, посылающего уведомление.
        /// </param>
        /// <param name="eventTypeId">
        /// Идентификатор типа события.
        /// </param>
        public void RiseEventOnESB(string clientId, string eventTypeId)
        {
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage
            {
                MessageTypeID = eventTypeId,
                ClientID = clientId
            };

            _receivingManager.AcceptMessage(serviceBusMessage);
        }

        /// <summary>
        /// Отправить сообщение в шину.
        /// </summary>
        /// <param name="message">Структура данных, описывающая отправляемое сообщение.</param>
        public void SendMessageToESB(MessageForESB message)
        {
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage
            {
                Body = message.Body,
                MessageTypeID = message.MessageTypeID,
                ClientID = message.ClientID,
                Tags = message.Tags,
                Attachment = message.Attachment,
                Priority = message.Priority
            };

            _receivingManager.AcceptMessage(serviceBusMessage);
        }

        /// <summary>
        /// Отправить сообщение в шину с заданным именем группы.
        /// </summary>
        /// <param name="message">Структура данных, описывающая отправляемое сообщение.</param>
        /// <param name="groupName">Имя группы.</param>
        public void SendMessageToESBWithUseGroup(MessageForESB message, string groupName)
        {
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage
            {
                Body = message.Body,
                MessageTypeID = message.MessageTypeID,
                ClientID = message.ClientID,
                Tags = message.Tags,
                Attachment = message.Attachment,
                Priority = message.Priority
            };

            _receivingManager.AcceptMessage(serviceBusMessage, groupName);
        }

        /// <summary>
        /// Подписать клиента на получение уведомлений о событии заданного типа по callback.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно подписать на уведомления.</param>
        /// <param name="eventTypeId">Идентификатор типа события, о котором нужно уведомлять клиента.</param>
        public void SubscribeClientForEventCallback(string clientId, string eventTypeId)
        {
            SubscribeClientForMessageCallback(clientId, eventTypeId);
        }

        /// <summary>
        /// Подписать клиента на получение сообщений заданного типа по callback.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно подписать на сообщения.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщений, на которые подписывается клиент.</param>
        public void SubscribeClientForMessageCallback(string clientId, string messageTypeId)
        {
            _subscriptionsManager.SubscribeOrUpdate(clientId, messageTypeId, true, TransportType.WCF);
        }

        /// <summary>
        /// Метод для проверки, что сервис доступен.
        /// </summary>
        /// <returns><c>true</c>, если сервис корректно работает.</returns>
        public bool IsUp()
        {
            return true;
        }
    }
}