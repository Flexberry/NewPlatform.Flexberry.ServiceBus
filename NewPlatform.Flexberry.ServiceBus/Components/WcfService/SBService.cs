namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.ServiceModel;
    using Components;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public partial class SBService
    {
        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly ISendingManager _sendingManager;

        private readonly IReceivingManager _receivingManager;

        private readonly IStatisticsService _statisticsService;

        private readonly IObjectRepository _objectRepository;

        public SBService(ISubscriptionsManager subscriptionsManager, ISendingManager sendingManager, IReceivingManager receivingManager, IStatisticsService statisticsService, IObjectRepository objectRepository)
        {
            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (sendingManager == null)
                throw new ArgumentNullException(nameof(sendingManager));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(objectRepository));

            _subscriptionsManager = subscriptionsManager;
            _sendingManager = sendingManager;
            _receivingManager = receivingManager;
            _statisticsService = statisticsService;
            _objectRepository = objectRepository;
        }
    }
}
