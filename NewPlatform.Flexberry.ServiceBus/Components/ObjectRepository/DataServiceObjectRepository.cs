namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

    using NewPlatform.Flexberry.ServiceBus.Components.ObjectRepository;

    using Npgsql;

    /// <summary>
    /// Default implementation of <see cref="IObjectRepository"/> using <see cref="IDataService"/>.
    /// </summary>
    internal sealed class DataServiceObjectRepository : BaseServiceBusComponent, IObjectRepository
    {
        /// <summary>
        /// The data service for loading objects.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Statistics service
        /// </summary>
        private readonly IStatisticsService _statisticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataServiceObjectRepository"/> class with
        /// specified data service.
        /// </summary>
        /// <param name="dataService">The data service for loading objects.</param>
        public DataServiceObjectRepository(IDataService dataService, IStatisticsService statisticsService)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            _dataService = dataService;
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Gets all service buses.
        /// </summary>
        /// <returns>The list of all stored remote services buses.</returns>
        public IEnumerable<Bus> GetAllServiceBuses()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(Bus), Bus.Views.ListView)).Cast<Bus>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetAllServiceBuses() load Шина");

            return dobjs;
        }

        /// <summary>
        /// Gets all message types.
        /// </summary>
        /// <returns>The list of all stored message types.</returns>
        public IEnumerable<MessageType> GetAllMessageTypes()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = _dataService.LoadObjects(LoadingCustomizationStruct.GetSimpleStruct(typeof(MessageType), MessageType.Views.ListView)).Cast<MessageType>();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetAllMessageTypes() load ТипСообщения");

            return dobjs;
        }

        /// <summary>
        /// Returns all sending restrictions.
        /// </summary>
        /// <returns>The list of all stored restrictions.</returns>
        public IEnumerable<SendingPermission> GetAllRestrictions()
        {
            var query = from x in _dataService.Query<SendingPermission>(SendingPermission.Views.ServiceBusView.Name)
                        select x;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = query.ToList();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetAllRestrictions() load OutboundMessageTypeRestriction");

            return dobjs;
        }

        /// <summary>
        /// Returns sending restrictions for client.
        /// </summary>
        /// <param name="senderId">Client's ID.</param>
        /// <returns>Restrictions for client.</returns>
        public IEnumerable<SendingPermission> GetRestrictionsForClient(string senderId)
        {
            var query = from x in _dataService.Query<SendingPermission>(SendingPermission.Views.ServiceBusView.Name)
                        where x.Client.ID == senderId
                        select x;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = query.ToList();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetRestrictionsForClient() load OutboundMessageTypeRestriction");

            return dobjs;
        }

        /// <summary>
        /// Returns sending restrictions for message type.
        /// </summary>
        /// <param name="messageTypeId">Message type's ID.</param>
        /// <returns>Restrictions for messge type.</returns>
        public IEnumerable<SendingPermission> GetRestrictionsForMsgType(string messageTypeId)
        {
            var query = from x in _dataService.Query<SendingPermission>(SendingPermission.Views.ServiceBusView.Name)
                        where x.MessageType.ID == messageTypeId
                        select x;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var dobjs = query.ToList();

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetRestrictionsForMsgType() load OutboundMessageTypeRestriction");

            return dobjs;
        }

        /// <summary>
        /// Create sending permission.
        /// </summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        public void CreateSendingPermission(string clientId, string messageTypeId)
        {
            CommonMetodsObjectRepository.CreateSendingPermission(clientId, messageTypeId, _dataService, _statisticsService);
        }

        /// <summary>
        /// Delete sending permission.
        /// </summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        public void DeleteSendingPermission(string clientId, string messageTypeId)
        {
            CommonMetodsObjectRepository.DeleteSendingPermission(clientId, messageTypeId, _dataService, _statisticsService);
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <returns>The list of all stored clients</returns>
        public IEnumerable<Client> GetAllClients()
        {
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Client), Client.Views.EditView);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] clients = _dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetAllClients() load Clients.");

            return clients.Cast<Client>().ToList();
        }

        /// <summary>
        /// Get restricting subscription.
        /// </summary>
        /// <param name="subscriptions">Client subscriptions.</param>
        /// <returns>Restricting subscription.</returns>
        public Subscription GetSubscriptionRestrictingQueue(IEnumerable<Subscription> subscriptions)
        {
            var subscriptionMessageTypeIds = string.Join(", ", subscriptions.Select(x => $"'{x.MessageType.ID}'").Distinct());
            var subscriptionRecipientIds = string.Join(", ", subscriptions.Select(x => $"'{x.Client.ID}'").Distinct());
            var messageGroups = new List<Tuple<string, int>>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
           
            if (_dataService is MSSQLDataService || _dataService.GetType().IsSubclassOf(typeof(MSSQLDataService)))
            {
                var query = @"SELECT t.[Ид], r.[Ид], COUNT(m.primaryKey) FROM [Сообщение] AS m 
                            INNER JOIN [ТипСообщения] AS t ON m.[ТипСообщения_m0] = t.primaryKey AND t.[Ид] IN (" + subscriptionMessageTypeIds + ") " +
                            "INNER JOIN [Клиент] AS r ON m.[Получатель_m0] = r.primaryKey AND r.[Ид] IN (" + subscriptionRecipientIds + ") " +
                            "GROUP BY t.[Ид], r.[Ид] ORDER BY 3 DESC";

                using (var connection = new SqlConnection(_dataService.CustomizationString))
                {
                    connection.Open();
                    var command = new SqlCommand(query, connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        messageGroups.Add(new Tuple<string, int>(reader.GetString(0), reader.GetInt32(2)));
                    }

                    reader.Close();
                    connection.Close();
                }
            }
            else if (_dataService is PostgresDataService || _dataService.GetType().IsSubclassOf(typeof(PostgresDataService)))
            {
                var query = "SELECT t.\"Ид\", r.\"Ид\", COUNT(m.primaryKey) FROM \"Сообщение\" AS m " +
                            "INNER JOIN \"ТипСообщения\" AS t ON m.\"ТипСообщения_m0\" = t.primaryKey AND t.\"Ид\" IN (" + subscriptionMessageTypeIds + ") " +
                            "INNER JOIN \"Клиент\" AS r ON m.\"Получатель_m0\" = r.primaryKey AND r.\"Ид\" IN (" + subscriptionRecipientIds + ") " +
                            "GROUP BY t.\"Ид\", r.\"Ид\" ORDER BY 3 DESC";

                using (var connection = new NpgsqlConnection(_dataService.CustomizationString))
                {
                    var command = new NpgsqlCommand(query, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        messageGroups.Add(new Tuple<string, int>(reader.GetString(0), reader.GetInt32(2)));
                    }

                    reader.Close();
                    connection.Close();
                }
            }

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            _statisticsService.NotifyAvgTimeSql(null, (int)time, "DataServiceObjectRepository.GetSubscriptionRestrictingQueue() load Subscription messages count.");

            foreach (var messageGroup in messageGroups)
            {
                var subscription = subscriptions.FirstOrDefault(x => x.MessageType.ID == messageGroup.Item1 && messageGroup.Item2 >= x.MaxQueueLength);
                if (subscription != null)
                {
                    return subscription;
                }
            }

            return null;
        }
    }
}