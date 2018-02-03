namespace NewPlatform.Flexberry.ServiceBus.Controllers
{
    using System;
    using System.Web.Http.Dependencies;
    using Components;

    public class RestDependencyResolver : RestDependencyScope, IDependencyResolver
    {
        private readonly ISendingManager _sendingManager;

        private readonly IReceivingManager _receivingManager;

        public RestDependencyResolver(ISendingManager sendingManager, IReceivingManager receivingManager)
            : base(sendingManager, receivingManager)
        {
            if (sendingManager == null)
                throw new ArgumentNullException(nameof(sendingManager));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            _sendingManager = sendingManager;
            _receivingManager = receivingManager;
        }

        public IDependencyScope BeginScope()
        {
            return new RestDependencyScope(_sendingManager, _receivingManager);
        }
    }
}
