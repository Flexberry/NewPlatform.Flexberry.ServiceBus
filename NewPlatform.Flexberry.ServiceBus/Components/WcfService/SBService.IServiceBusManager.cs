namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using ICSSoft.STORMNET;

    /// <summary>
    /// В данном классе шина выступает в роли клиента, принимая сообщения
    /// </summary>
    public partial class SBService : IServiceBusManager
    {
        /// <summary>
        /// Создать тип сообщения в шине.
        /// </summary>
        /// <param name="messageType">Тип сообщения.</param>
        void IServiceBusManager.CreateMessageType(ServiceBusMessageType messageType)
        {
            _subscriptionsManager.CreateMessageType(messageType);
        }

        /// <summary>
        /// Обновить тип сообщения в шине.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщения, который нужно обновить.</param>
        /// <param name="messageType">Новые свойства типа сообщения.</param>
        void IServiceBusManager.UpdateMessageType(string messageTypeId, ServiceBusMessageType messageType)
        {
            _subscriptionsManager.UpdateMessageType(messageTypeId, messageType);
        }

        /// <summary>
        /// Удалить тип сообщения из шины.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщения, который нужно удалить.</param>
        void IServiceBusManager.DeleteMessageType(string messageTypeId)
        {
            _subscriptionsManager.DeleteMessageType(messageTypeId);
        }

        /// <summary>
        /// Получить все типы сообщений в шине.
        /// </summary>
        /// <returns>Все типы сообщений.</returns>
        IEnumerable<ServiceBusMessageType> IServiceBusManager.GetMessageTypes()
        {
            IEnumerable<MessageType> messageTypes = _objectRepository.GetAllMessageTypes();
            List<ServiceBusMessageType> serviceBusMessageTypes = new List<ServiceBusMessageType>();
            foreach (var messageType in messageTypes)
            {
                ServiceBusMessageType serviceBusMessageType = new ServiceBusMessageType
                {
                    Id = messageType.ID,
                    Name = messageType.Name,
                    Description = messageType.Description

                };

                serviceBusMessageTypes.Add(serviceBusMessageType);
            }

            return serviceBusMessageTypes;
        }

        /// <summary>
        /// Создать клиента в шине.
        /// </summary>
        /// <param name="client">Клиент.</param>
        void IServiceBusManager.CreateClient(ServiceBusClient client)
        {
            _subscriptionsManager.CreateClient(client.Id, client.Name, client.Address);
        }

        /// <summary>
        /// Удалить клиента из шины.
        /// </summary>
        /// <param name="clientId">
        /// Идентификатор клиента, которого нужно удалить.
        /// </param>
        void IServiceBusManager.DeleteClient(string clientId)
        {
            _subscriptionsManager.DeleteClient(clientId);
        }

        /// <summary>
        /// Обновить клиента в шине.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента, которого нужно обновить.</param>
        /// <param name="client">Клиент.</param>
        void IServiceBusManager.UpdateClient(string clientId, ServiceBusClient client)
        {
            _subscriptionsManager.UpdateClient(clientId, client);
        }

        /// <summary>
        /// Получить всех клиентов в шине.
        /// </summary>
        /// <returns>Все клиенты.</returns>
        IEnumerable<ServiceBusClient> IServiceBusManager.GetClients()
        {
            var allClients = _objectRepository.GetAllClients();
            List<ServiceBusClient> serviceBusClients = new List<ServiceBusClient>();
            foreach (var client in allClients)
            {
                ServiceBusClient serviceBusClient = new ServiceBusClient
                {
                    Id = client.ID,
                    Name = client.Name,
                    Description = client.Description,
                    Address = client.Address,
                    ConnectionsLimit = client.ConnectionsLimit,
                    DnsIdentity = client.DnsIdentity,
                    SequentialSent = client.SequentialSent

                };

                serviceBusClients.Add(serviceBusClient);
            }

            return serviceBusClients;
        }

        /// <summary>
        /// Создать подписку в шине.
        /// </summary>
        /// <param name="subscription">Подписка.</param>
        void IServiceBusManager.CreateSubscription(ServiceBusSubscription subscription)
        {
            TransportType transportType;
            EnumCaption.TryGetValueFor(subscription.SendBy, out transportType);
            bool callback = subscription.Callback ?? false;

            _subscriptionsManager.SubscribeOrUpdate(subscription.ClientId, subscription.MessageTypeId, callback, transportType, subscription.ExpiryDate);
        }

        /// <summary>
        /// Обновить подписку в шине.
        /// </summary>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно обновить.</param>
        /// <param name="subscription">Подписка.</param>
        void IServiceBusManager.UpdateSubscription(string subscriptionId, ServiceBusSubscription subscription)
        {
            _subscriptionsManager.UpdateSubscription(subscriptionId, subscription);
        }

        /// <summary>
        /// Удалить подписку из шины.
        /// </summary>
        /// <param name="subscriptionId">Идентификатор подписки, которую нужно удалить.</param>
        void IServiceBusManager.DeleteSubscription(string subscriptionId)
        {
            _subscriptionsManager.DeleteSubscription(subscriptionId);
        }

        /// <summary>
        /// Получить все подписки в шине.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Все подписки клиента.</returns>
        IEnumerable<ServiceBusSubscription> IServiceBusManager.GetSubscriptions(string clientId)
        {
            IEnumerable<Subscription> subscriptions = _subscriptionsManager.GetSubscriptions(false);
            IEnumerable<Subscription> clientSubscriptions = subscriptions.Where(x => x.Client.ID == clientId);
            List<ServiceBusSubscription> serviceBusSubscriptions = new List<ServiceBusSubscription>();
            foreach (var subscription in clientSubscriptions)
            {
                ServiceBusSubscription serviceBusSubscription = new ServiceBusSubscription
                {
                    SendBy = subscription.TransportType.ToString(),
                    Callback = subscription.IsCallback,
                    ClientId = subscription.Client.ID,
                    MessageTypeId = subscription.MessageType.ID,
                    Description = subscription.Description,
                    ExpiryDate = subscription.ExpiryDate
                };

                serviceBusSubscriptions.Add(serviceBusSubscription);
            }

            return serviceBusSubscriptions;
        }

        /// <summary>
        /// Создать раздешение на отправку.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        void IServiceBusManager.CreateSendingPermission(string clientId, string messageTypeId)
        {
            _objectRepository.CreateSendingPermission(clientId, messageTypeId);
        }

        /// <summary>
        /// Удалить раздешение на отправку.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        void IServiceBusManager.DeleteSendingPermission(string clientId, string messageTypeId)
        {
            _objectRepository.DeleteSendingPermission(clientId, messageTypeId);
        }

        /// <summary>
        /// Получить все разрешения на отправку.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Все разрешения на отправку клиента.</returns>
        string[] IServiceBusManager.GetSendingPermissions(string clientId)
        {
            IEnumerable<SendingPermission> sendingPermissions = _objectRepository.GetAllRestrictions();
            IEnumerable<SendingPermission> clientSendingPermissions = sendingPermissions.Where(x => x.Client.ID == clientId);
            List<string> serviceBusSendingPermissions = new List<string>();
            foreach (var sendingPermission in clientSendingPermissions)
            {
                serviceBusSendingPermissions.Add(sendingPermission.MessageType.ID);
            }

            return serviceBusSendingPermissions.ToArray();
        }
    }
}