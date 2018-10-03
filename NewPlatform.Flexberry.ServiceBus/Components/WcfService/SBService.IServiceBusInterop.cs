namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Содержит методы для работы с событиями, подписками, типами сообщений
    /// </summary>
    public partial class SBService : IServiceBusInterop
    {
        /// <summary>
        /// Создать новый тип событий.
        /// </summary>
        /// <param name="evntType">Информация о создаваемом типе событий.</param>
        public void AddNewEvntType(NameCommentStruct evntType)
        {
            _subscriptionsManager.CreateEventType(evntType);
        }

        /// <summary>
        /// Создать новый тип сообщений.
        /// </summary>
        /// <param name="msgType">Информация о создаваемом типе сообщений.</param>
        public void AddNewMsgType(NameCommentStruct msgType)
        {
            _subscriptionsManager.CreateMessageType(msgType);
        }

        /// <summary>
        /// Получить типы событий, на которые подписан клиент.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, для которого нужно получить типы событий.</param>
        /// <returns>Информация о найденных типах событий.</returns>
        public NameCommentStruct[] GetEvntTypesFromBus(string clientId)
        {
            return _subscriptionsManager.GetSubscriptions(clientId).Select(x => new NameCommentStruct
            {
                Id = x.MessageType.ID,
                Name = x.MessageType.Name,
                Comment = x.MessageType.Description,
            }).ToArray();
        }

        /// <summary>
        /// Получить типы сообщений, на которые подписан клиент.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, для которого нужно найти типы сообщений.</param>
        /// <returns>Информация о найденных типах сообщений.</returns>
        public NameCommentStruct[] GetMsgTypesFromBus(string clientId)
        {
            return _subscriptionsManager.GetSubscriptions(clientId).Select(x => new NameCommentStruct
            {
                Id = x.MessageType.ID,
                Name = x.MessageType.Name,
                Comment = x.MessageType.Description,
            }).ToArray();
        }

        /// <summary>
        /// Продлить все подписки на события указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписки которого нужно обновить.</param>
        public void UpdateClientSubscribesForEvnts(string clientId)
        {
            _subscriptionsManager.UpdateAllSubscriptions(clientId);
        }

        /// <summary>
        /// Продлить все подписки на сообщения указанного клиента.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, подписки которого нужно обновить.</param>
        public void UpdateClientSubscribesForMsgs(string clientId)
        {
            _subscriptionsManager.UpdateAllSubscriptions(clientId);
        }

        /// <summary>
        /// Returns current state.
        /// </summary>
        /// <returns>Information about messages in the process of sending.</returns>
        public MessageInfo[] GetCurrentState()
        {
            return _statisticsService.GetCurrentState();
        }
    }
}