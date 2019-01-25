namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Xunit;

    public class CachedDataServiceObjectRepositoryTest : BaseServiceBusIntegratedTest
    {
        public CachedDataServiceObjectRepositoryTest()
            : base("CObjRepoCach")
        {
        }

        /// <summary>
        /// Testing GetAllServiceBuses method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetAllServiceBuses()
        {
            foreach (var dataService in DataServices)
            {
                DataObject[] objsToUpdate =
                {
                    new Bus() { ID = "Bus 1", Name = "Bus name 1", ManagerAddress = "http://localhost:12343/SBService" },
                    new Bus() { ID = "Bus 2", Name = "Bus name 2", ManagerAddress = "http://localhost:12344/SBService" }
                };

                DataObject[] additionalObjsToUpdate =
                {
                    new Bus() { ID = "Bus 3", Name = "Bus name 3", ManagerAddress = "http://localhost:12345/SBService" },
                    new Bus() { ID = "Bus 4", Name = "Bus name 4", ManagerAddress = "http://localhost:12346/SBService" }
                };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                TestLoadingAllData(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetAllServiceBuses, updatePeriod, new[] { "ID", "Name", "ManagerAddress" });
            }
        }

        /// <summary>
        /// Testing GetAllMessageTypes method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetAllMessageTypes()
        {
            foreach (var dataService in DataServices)
            {
                DataObject[] objsToUpdate =
                {
                    new MessageType() { ID = "Message type 1", Name = "Message type name 1" },
                    new MessageType() { ID = "Message type 2", Name = "Message type name 2" }
                };

                DataObject[] additionalObjsToUpdate =
                {
                    new MessageType() { ID = "Message type 3", Name = "Message type name 3" },
                    new MessageType() { ID = "Message type 4", Name = "Message type name 4" }
                };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                TestLoadingAllData(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetAllMessageTypes, updatePeriod, new[] { "ID", "Name" });
            }
        }

        /// <summary>
        /// Testing TestGetAllRestrictions method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetAllRestrictions()
        {
            foreach (var dataService in DataServices)
            {
                var client1 = new Client() { ID = "Client 1", Name = "Client name 1" };
                var client2 = new Client() { ID = "Client 2", Name = "Client name 2" };
                var client3 = new Client() { ID = "Client 3", Name = "Client name 3" };
                var client4 = new Client() { ID = "Client 4", Name = "Client name 4" };
                var messageType1 = new MessageType() { ID = "Message type 1", Name = "Message type name 1" };
                var messageType2 = new MessageType() { ID = "Message type 2", Name = "Message type name 2" };
                var messageType3 = new MessageType() { ID = "Message type 3", Name = "Message type name 3" };
                var messageType4 = new MessageType() { ID = "Message type 4", Name = "Message type name 4" };

                var sendingPermission1 = new SendingPermission() { Client = client1, MessageType = messageType1 };
                var sendingPermission2 = new SendingPermission() { Client = client2, MessageType = messageType2 };
                var sendingPermission3 = new SendingPermission() { Client = client3, MessageType = messageType3 };
                var sendingPermission4 = new SendingPermission() { Client = client4, MessageType = messageType4 };

                DataObject[] objsToUpdate = { client1, client2, messageType1, messageType2, sendingPermission1, sendingPermission2 };

                DataObject[] additionalObjsToUpdate = { client3, client4, messageType3, messageType4, sendingPermission3, sendingPermission4 };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                TestLoadingAllData(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetAllRestrictions, updatePeriod, new[] { "Client.ID", "MessageType.ID" });
            }
        }

        /// <summary>
        /// Testing TestGetAllClients method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetAllClients()
        {
            foreach (var dataService in DataServices)
            {
                var client1 = new Client() { ID = "Client 1", Name = "Client name 1" };
                var client2 = new Client() { ID = "Client 2", Name = "Client name 2" };
                var client3 = new Client() { ID = "Client 3", Name = "Client name 3" };
                var client4 = new Client() { ID = "Client 4", Name = "Client name 4" };

                DataObject[] objsToUpdate = { client1, client2 };

                DataObject[] additionalObjsToUpdate = { client3, client4 };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                TestLoadingAllData(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetAllClients, updatePeriod, new[] { "ID", "Name" });
            }
        }

        /// <summary>
        /// Testing TestGetRestrictionsForClient method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetRestrictionsForClient()
        {
            foreach (var dataService in DataServices)
            {
                var client1 = new Client() { ID = "Client 1", Name = "Client name 1" };
                var client2 = new Client() { ID = "Client 2", Name = "Client name 2" };
                var client3 = new Client() { ID = "Client 3", Name = "Client name 3" };
                var messageType1 = new MessageType() { ID = "Message type 1", Name = "Message type name 1" };
                var messageType2 = new MessageType() { ID = "Message type 2", Name = "Message type name 2" };
                var messageType3 = new MessageType() { ID = "Message type 3", Name = "Message type name 3" };

                var sendingPermission1 = new SendingPermission() { Client = client1, MessageType = messageType1 };
                var sendingPermission2 = new SendingPermission() { Client = client2, MessageType = messageType2 };
                var sendingPermission3 = new SendingPermission() { Client = client3, MessageType = messageType3 };
                var sendingPermission4 = new SendingPermission() { Client = client1, MessageType = messageType2 };

                DataObject[] objsToUpdate = { client1, client2, messageType1, messageType2, sendingPermission1, sendingPermission2 };

                DataObject[] additionalObjsToUpdate = { client3, messageType3, sendingPermission3, sendingPermission4 };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                var clientId = client1.ID;

                TestLoadingDataWithCondition(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetRestrictionsForClient, clientId, updatePeriod, "Client.ID");
            }
        }

        /// <summary>
        /// Testing TestGetRestrictionsForMsgType method with empty data, non-empty data and data stored after cache updating.
        /// </summary>
        [Fact]
        public void TestGetRestrictionsForMsgType()
        {
            foreach (var dataService in DataServices)
            {
                var client1 = new Client() { ID = "Client 1", Name = "Client name 1" };
                var client2 = new Client() { ID = "Client 2", Name = "Client name 2" };
                var client3 = new Client() { ID = "Client 3", Name = "Client name 3" };
                var messageType1 = new MessageType() { ID = "Message type 1", Name = "Message type name 1" };
                var messageType2 = new MessageType() { ID = "Message type 2", Name = "Message type name 2" };
                var messageType3 = new MessageType() { ID = "Message type 3", Name = "Message type name 3" };

                var sendingPermission1 = new SendingPermission() { Client = client1, MessageType = messageType1 };
                var sendingPermission2 = new SendingPermission() { Client = client2, MessageType = messageType2 };
                var sendingPermission3 = new SendingPermission() { Client = client3, MessageType = messageType3 };
                var sendingPermission4 = new SendingPermission() { Client = client1, MessageType = messageType2 };

                DataObject[] objsToUpdate = { client1, client2, messageType1, messageType2, sendingPermission1, sendingPermission2 };

                DataObject[] additionalObjsToUpdate = { client3, messageType3, sendingPermission3, sendingPermission4 };

                var updatePeriod = 3000;
                var component = new CachedDataServiceObjectRepository(GetMockLogger(),
                    (IDataService)dataService.Clone(), GetMockStatisticsService())
                { UpdatePeriodMilliseconds = updatePeriod };

                var messageTypeId = messageType2.ID;

                TestLoadingDataWithCondition(dataService, objsToUpdate, additionalObjsToUpdate, component, component.GetRestrictionsForMsgType, messageTypeId, updatePeriod, "MessageType.ID");
            }
        }

        private void TestLoadingAllData<TDataObjectType>(
            IDataService dataService,
            DataObject[] objsToUpdate,
            DataObject[] additionalObjsToUpdate,
            CachedDataServiceObjectRepository component,
            Func<IEnumerable<TDataObjectType>> methodUnderTest,
            int updatePeriod,
            string[] propertiesToCheck)
            where TDataObjectType : DataObject
        {
            // Arrange.
            dataService.UpdateObjects(ref objsToUpdate);
            var objsToCheckCount = ObjectRepositoryTestHelper.GetObjectsOfSpecifiedTypeCount<TDataObjectType>(objsToUpdate);
            var additionalObjsToCheckCount = ObjectRepositoryTestHelper.GetObjectsOfSpecifiedTypeCount<TDataObjectType>(additionalObjsToUpdate);

            // Act and Assert.
            var actualList = methodUnderTest();

            // If component is not started then no data should be stored in cache.
            Assert.Equal(0, actualList.Count());

            component.Prepare();
            actualList = methodUnderTest();

            // Ititially stored data shuould be loaded to cache after preparing component.
            Assert.Equal(objsToCheckCount, actualList.Count());
            ObjectRepositoryTestHelper.CheckPropertiesOfAllObjects(actualList, objsToUpdate, propertiesToCheck);

            // Add more buses to database.
            dataService.UpdateObjects(ref additionalObjsToUpdate);

            component.Start();
            Thread.Sleep(updatePeriod + 1000);
            actualList = methodUnderTest();

            // Ititially stored data and additional data shuould be loaded to cache after starting component.
            Assert.Equal(objsToCheckCount + additionalObjsToCheckCount, actualList.Count());
            ObjectRepositoryTestHelper.CheckPropertiesOfAllObjects(actualList, objsToUpdate, propertiesToCheck);
            ObjectRepositoryTestHelper.CheckPropertiesOfAllObjects(actualList, additionalObjsToUpdate, propertiesToCheck);

            component.Stop();
            component.ClearCache();
        }

        private void TestLoadingDataWithCondition<TDataObjectType, TParamType>(
            IDataService dataService,
            DataObject[] objsToUpdate,
            DataObject[] additionalObjsToUpdate,
            CachedDataServiceObjectRepository component,
            Func<TParamType, IEnumerable<TDataObjectType>> methodUnderTest,
            TParamType methodParamValue,
            int updatePeriod,
            string propertyToCheck)
            where TDataObjectType : DataObject
        {
            // Arrange.
            dataService.UpdateObjects(ref objsToUpdate);
            var objsToCheckCount = ObjectRepositoryTestHelper.GetObjectsOfSpecifiedTypeCount<TDataObjectType>(objsToUpdate);
            var additionalObjsToCheckCount = ObjectRepositoryTestHelper.GetObjectsOfSpecifiedTypeCount<TDataObjectType>(additionalObjsToUpdate);

            // Act and Assert.
            var actualList = methodUnderTest(methodParamValue);

            // If component is not started then no data should be stored in cache.
            Assert.Equal(0, actualList.Count());

            component.Prepare();
            actualList = methodUnderTest(methodParamValue);

            // Ititially stored data shuould be loaded to cache after preparing component.
            ObjectRepositoryTestHelper.CheckValue(actualList, propertyToCheck, methodParamValue, 1);

            // Add more buses to database.
            dataService.UpdateObjects(ref additionalObjsToUpdate);

            component.Start();
            Thread.Sleep(updatePeriod + 1000);
            actualList = methodUnderTest(methodParamValue);

            // Ititially stored data and additional data shuould be loaded to cache after starting component.
            ObjectRepositoryTestHelper.CheckValue(actualList, propertyToCheck, methodParamValue, 2);

            component.Stop();
            component.ClearCache();
        }
    }
}
