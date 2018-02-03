namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.Business.LINQProvider;

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
    }
}