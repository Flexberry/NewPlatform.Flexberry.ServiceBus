namespace NewPlatform.Flexberry.ServiceBus.RabbitMQTests
{
    using System;
    using System.ServiceModel;
    using System.Threading;
    using NewPlatform.Flexberry.ServiceBus.ClientTools;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestCallbackClient : ICallbackSubscriber
    {
        public string ClientId { get; set; } = "CallbackReceiverId";

        private int receivedMsgCount = 0;
        public int ExpectedMessageCount { get; set; }
        public ManualResetEvent ResetEvent { get; set; }

        public static void Configure(ServiceConfiguration config)
        {
            config.AddServiceEndpoint(typeof(ICallbackSubscriber), new WSHttpBinding("CallbackClientBinding"), "http://localhost:12345/SbListener");
        }

        public void ResetMessageCount() => receivedMsgCount = 0;

        public void AcceptMessage(MessageFromESB msg)
        {
            try
            {
                if (msg.Body == "ThrowException")
                    throw new Exception("TestException");
            }
            finally
            {
                receivedMsgCount++;
                if (receivedMsgCount == ExpectedMessageCount)
                {
                    ResetEvent?.Set();
                }
            }
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
