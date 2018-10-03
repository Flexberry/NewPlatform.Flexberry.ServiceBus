namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Base abstract implementation of <see cref="IReceivingManager"/>.
    /// </summary>
    internal abstract class BaseReceivingManager : BaseServiceBusComponent, IReceivingManager
    {
        public abstract void AcceptMessage(MessageForESB message);

        public abstract void AcceptMessage(MessageForESB message, string groupName);

        public abstract void RaiseEvent(string clientId, string eventTypeId);
    }
}
