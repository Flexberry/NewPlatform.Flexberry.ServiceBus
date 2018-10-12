namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Base abstract implementation of <see cref="IReceivingManager"/>.
    /// </summary>
    internal abstract class BaseReceivingManager : BaseServiceBusComponent, IReceivingManager
    {
        public abstract void AcceptMessage(ServiceBusMessage message);

        public abstract void AcceptMessage(ServiceBusMessage message, string groupName);
    }
}
