using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    public interface ISubscriptionSynchronizer : NewPlatform.Flexberry.ServiceBus.Components.IServiceBusComponent
    {
        int UpdatePeriodMilliseconds { get; set; }

        void UpdateMqSubscriptions(List<Subscription> mqSubscriptions, List<Subscription> esbSubscriptions);

        void UpdateEsbSubscriptions(List<Subscription> mqSubscriptions, List<Subscription> esbSubscriptions);

        void SynchronizeSendingPermissions(string clientId = null);
    }
}
