namespace NewPlatform.Flexberry.ServiceBus.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;
    using Components;

    public class RestDependencyScope : IDependencyScope
    {
        private readonly ISendingManager _sendingManager;

        private readonly IReceivingManager _receivingManager;

        public RestDependencyScope(ISendingManager sendingManager, IReceivingManager receivingManager)
        {
            if (sendingManager == null)
                throw new ArgumentNullException(nameof(sendingManager));

            if (receivingManager == null)
                throw new ArgumentNullException(nameof(receivingManager));

            _sendingManager = sendingManager;
            _receivingManager = receivingManager;
        }

        public void Dispose()
        {

        }

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(RestServiceController) ? new RestServiceController(_sendingManager, _receivingManager) : null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>() { GetService(serviceType) };
        }
    }
}
