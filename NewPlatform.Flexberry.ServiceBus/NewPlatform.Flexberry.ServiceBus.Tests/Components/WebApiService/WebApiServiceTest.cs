namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using Xunit;

    /// <summary>
    /// Tests WebApi component.
    /// </summary>
    [Collection("WebAPITests")]
    public class WebApiServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB WebApi component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            using (var service = new WebApiService("http://localhost:12351/RestService", GetMockSendingManager(), GetMockReceivingManager()))
            {
                RunSBComponentFullCycle(service);
            }
        }

        /// <summary>
        /// Accessing WebApi services is not blocked.
        /// </summary>
        [Fact]
        public void TestAccessingWebApiServices()
        {
            // Arrange.
            var log = new List<HttpStatusCode>();
            var baseAddress = "http://localhost:12352/RestService";
            ThreadStart act = () =>
            {
                var httpClient = new HttpClient();
                var response = httpClient.GetAsync($"{baseAddress}/Message/{Guid.NewGuid():D}");
                log.Add(response.Result.StatusCode);
            };

            var sendingManager = GetMockSendingManager();
            var receivingManager = GetMockReceivingManager();
            var service = new WebApiService(baseAddress, sendingManager, receivingManager);

            // Act.
            using (service)
            {
                service.Start();

                Thread thread1 = new Thread(act);
                Thread thread2 = new Thread(act);

                thread1.Start();
                thread2.Start();
                thread1.Join();
                thread2.Join();

                service.Stop();
            }

            // Assert.
            Assert.Equal(2, log.Count);
            foreach (var statusCode in log)
            {
                Assert.Equal(HttpStatusCode.OK, statusCode);
            }
        }
    }
}
