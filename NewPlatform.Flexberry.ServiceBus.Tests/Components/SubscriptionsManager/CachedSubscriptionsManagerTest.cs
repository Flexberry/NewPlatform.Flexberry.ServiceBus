namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Linq;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests CachedSubscriptionsManager component.
    /// </summary>
    public class CachedSubscriptionsManagerTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB CachedSubscriptionsManager component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new CachedSubscriptionsManager(GetMockLogger(), GetMockDataService(), GetMockStatisticsService());

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Testing UpdateFromDb timer.
        /// </summary>
        [Fact]
        public void TestUpdateFromDb()
        {
            // Arrange.
            var dataServiceMock = new Mock<IDataService>();
            var service = new CachedSubscriptionsManager(GetMockLogger(), dataServiceMock.Object, GetMockStatisticsService())
            {
                UpdatePeriodMilliseconds = 100
            };
            service.Prepare();

            // Act.
            service.Start();
            Thread.Sleep(150);
            service.Stop();

            // Assert.
            dataServiceMock.Verify(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(Subscription))), Times.AtLeast(2));
        }

        /// <summary>
        /// Testing message type creation.
        /// </summary>
        [Fact]
        public void TestMessageTypeCreate()
        {
            // Arrange.
            var messageTypeInfo = new ServiceBusMessageType { Id = "123", Name = "TestMessageType", Description = "ForTest" };
            var dataServiceMock = new Mock<IDataService>();
            var service = new CachedSubscriptionsManager(GetMockLogger(), dataServiceMock.Object, GetMockStatisticsService());

            // Act.
            service.CreateMessageType(messageTypeInfo);

            // Assert.
            dataServiceMock.Verify(f => f.UpdateObject(It.Is<MessageType>(t => t.ID == messageTypeInfo.Id && t.Name == messageTypeInfo.Name && t.Description == messageTypeInfo.Description)), Times.Once);
        }

        /// <summary>
        /// Testing client creation.
        /// </summary>
        [Fact]
        public void TestClientCreate()
        {
            // Arrange.
            const string clientId = "03FE3B98-2D09-4032-A5BF-03BEDF86F4F4";
            const string clientName = "SucessClient";
            const string clientAddress = "TestAddress";
            var dataServiceMock = new Mock<IDataService>();
            var service = new CachedSubscriptionsManager(GetMockLogger(), dataServiceMock.Object, GetMockStatisticsService());

            // Act.
            service.CreateClient(clientId, clientName, clientAddress);

            // Assert.
            dataServiceMock.Verify(f => f.UpdateObject(It.Is<Client>(t => t.ID == clientId && t.Name == clientName && t.Address == clientAddress)), Times.Once);
            Assert.Throws<ArgumentNullException>(() => service.CreateClient(null, clientName, clientAddress));
        }

        /// <summary>
        /// Testing subscriptions loading.
        /// </summary>
        [Fact]
        public void TestSubscriptionsLoading()
        {
            // Arrange.
            const string client1Id = "03FE3B98-2D09-4032-A5BF-03BEDF86F4F4";
            const string client2Id = "C94B558A-D961-4ABA-8F67-C52AE377FFA5";
            const string messageType1Id = "EB6EC229-5E93-4B76-9993-5A1589787421";
            const string messageType2Id = "C8802C67-AC1B-497C-A707-5FF4191E0083";
            const string messageType3Id = "BC3F54C6-4E2F-43DA-B124-A0771F8F200C";
            var subscriptions = new DataObject[]
            {
                new Subscription() { Client = new Client() { __PrimaryKey = Guid.Parse(client1Id) }, MessageType = new MessageType() { __PrimaryKey = Guid.Parse(messageType1Id) }, ExpiryDate = new DateTime(DateTime.Now.Ticks + 100000000), IsCallback = true },
                new Subscription() { Client = new Client() { __PrimaryKey = Guid.Parse(client1Id) }, MessageType = new MessageType() { __PrimaryKey = Guid.Parse(messageType3Id) }, ExpiryDate = new DateTime(DateTime.Now.Ticks - 100000000), IsCallback = true },
                new Subscription() { Client = new Client() { __PrimaryKey = Guid.Parse(client1Id) }, MessageType = new MessageType() { __PrimaryKey = Guid.Parse(messageType2Id) }, ExpiryDate = new DateTime(DateTime.Now.Ticks + 100000000), IsCallback = false },
                new Subscription() { Client = new Client() { __PrimaryKey = Guid.Parse(client2Id) }, MessageType = new MessageType() { __PrimaryKey = Guid.Parse(messageType2Id) }, ExpiryDate = new DateTime(DateTime.Now.Ticks + 100000000), IsCallback = false }
            };
            var dataServiceMock = new Mock<IDataService>();
            dataServiceMock.Setup(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(Subscription)))).Returns(subscriptions);
            var service = new CachedSubscriptionsManager(GetMockLogger(), dataServiceMock.Object, GetMockStatisticsService());
            service.Prepare();

            // Act && Assert.
            var subs = service.GetSubscriptions();
            Assert.Equal(subs.Count(), 3);

            subs = service.GetSubscriptions(false);
            Assert.Equal(subs.Count(), 4);

            subs = service.GetSubscriptions(client2Id);
            Assert.Equal(subs.Count(), 1);
            Assert.True(subs.All(sub => Guid.Parse(sub.Client.__PrimaryKey.ToString()) == Guid.Parse(client2Id)));

            subs = service.GetSubscriptions(client2Id, false);
            Assert.Equal(subs.Count(), 1);
            Assert.True(subs.All(sub => Guid.Parse(sub.Client.__PrimaryKey.ToString()) == Guid.Parse(client2Id)));

            subs = service.GetCallbackSubscriptions();
            Assert.Equal(subs.Count(), 1);
            Assert.True(subs.All(sub => sub.IsCallback));

            subs = service.GetCallbackSubscriptions(false);
            Assert.Equal(subs.Count(), 2);
            Assert.True(subs.All(sub => sub.IsCallback));

            subs = service.GetSubscriptionsForMsgType(messageType2Id, client2Id);
            Assert.Equal(subs.Count(), 1);
            Assert.True(subs.All(sub => Guid.Parse(sub.MessageType.__PrimaryKey.ToString()) == Guid.Parse(messageType2Id)));
        }
    }
}
