namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
#if NETFRAMEWORK
    using System.ServiceModel;
#endif
    using NewPlatform.Flexberry.ServiceBus.Components;

#if NETFRAMEWORK
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
#endif
    public partial class SBService
    {
        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly ISendingManager _sendingManager;

        private readonly IReceivingManager _receivingManager;

        private readonly IStatisticsService _statisticsService;

        private readonly IObjectRepository _objectRepository;

        public SBService(ISubscriptionsManager subscriptionsManager, ISendingManager sendingManager, IReceivingManager receivingManager, IStatisticsService statisticsService, IObjectRepository objectRepository)
        {
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _sendingManager = sendingManager ?? throw new ArgumentNullException(nameof(sendingManager));
            _receivingManager = receivingManager ?? throw new ArgumentNullException(nameof(receivingManager));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
        }
    }
}
