[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
namespace NewPlatform.Flexberry.ServiceBus.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET.Business;
    using Moq;

    /// <summary>
    /// Base class for service bus unit tests.
    /// </summary>
    public abstract class BaseServiceBusTest
    {
        protected virtual ILogger GetMockLogger()
        {
            var mock = new Mock<ILogger>();
            mock
                .Setup(l => l.LogDebugMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((title, message) => Debug.WriteLine($"[{DateTime.Now}] {title}: {message}"));

            mock
                .Setup(l => l.LogUnhandledException(It.IsAny<Exception>(), It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<Exception, Message, string, string>((e, message, s, s1) => Debug.WriteLine($"[{DateTime.Now}] Exception: {e.Message}"));

            return mock.Object;
        }

        protected virtual IReceivingManager GetMockReceivingManager()
        {
            return new Mock<IReceivingManager>().Object;
        }

        protected virtual ISendingManager GetMockSendingManager()
        {
            return new Mock<ISendingManager>().Object;
        }

        protected virtual ISubscriptionsManager GetMockSubscriptionManager()
        {
            return new Mock<ISubscriptionsManager>().Object;
        }

        protected virtual IDataService GetMockDataService()
        {
            return new Mock<IDataService>().Object;
        }

        protected virtual IObjectRepository GetMockObjectRepository()
        {
            return new Mock<IObjectRepository>().Object;
        }

        protected virtual IStatisticsService GetMockStatisticsService()
        {
            return new Mock<IStatisticsService>().Object;
        }

        protected void RunSBComponentFullCycle(IServiceBusComponent component)
        {
            RunSBComponentFullCycle(component, 0);
        }

        protected void RunSBComponentFullCycle(IServiceBusComponent component, int sleepTime)
        {
            component.Prepare();
            Thread.Sleep(sleepTime);
            component.Start();
            Thread.Sleep(sleepTime);
            component.Stop();
            Thread.Sleep(sleepTime);
            component.AfterStop();
        }

        protected void RunSBComponentAfterStart<T>(T component, Action<T> action) where T : IServiceBusComponent
        {
            component.Prepare();
            component.Start();

            action(component);

            component.Stop();
            component.AfterStop();
        }
    }
}
