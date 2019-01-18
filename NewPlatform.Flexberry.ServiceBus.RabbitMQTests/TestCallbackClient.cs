namespace NewPlatform.Flexberry.ServiceBus.RabbitMQTests
{
    using System;
    using System.ServiceModel;
    using NewPlatform.Flexberry.ServiceBus.ClientTools;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestCallbackClient : ICallbackSubscriber
    {
        public string ClientId { get; set; } = "CallbackReceiverId";

        public static void Configure(ServiceConfiguration config)
        {
            config.AddServiceEndpoint(typeof(ICallbackSubscriber), new WSHttpBinding("CallbackClientBinding"), "http://localhost:12345/SbListener");
        }

        public void AcceptMessage(MessageFromESB msg)
        {
            if (msg.Body == "ThrowException")
                throw new Exception("TestException");
        }

        public void RiseEvent(string eventTypeId)
        {

        }

        public string GetSourceId()
        {
            return ClientId;
        }
    }
}
