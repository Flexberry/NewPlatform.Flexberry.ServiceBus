namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml;
    using Flexberry.ServiceBus.Components;
    using Flexberry.ServiceBus.Components.Rerouter;
    using Flexberry.ServiceBus.Components.RerouterConfiguration;
    using Moq;
    using Xunit;

    public class RerouterServiceTest : BaseServiceBusTest
    {
        /// <summary>
        /// Run SB RerouterService component full cycle.
        /// </summary>
        [Fact]
        public void TestStartStop()
        {
            var service = new RerouterService(
                GetMockSubscriptionManager(),
                new Mock<IStatisticsService>().Object,
                GetMockLogger());

            RunSBComponentFullCycle(service);
        }

        /// <summary>
        /// Testing RerouteRequest method.
        /// </summary>
        [Fact]
        public void TestRerouteRequest()
        {
            // Arrange.
            var config = MessageRerouterConfiguration.Current;
            var client1Id = config.ReceiverId;
            var client2Id = config.SenderId;
            var messageType1Id = config.Recievers[0].SbRequestType;
            var messageType2Id = config.Recievers[0].SbResponseType;
            Guid client1Pk = Guid.Parse(client1Id);
            Guid client2Pk = Guid.Parse(client2Id);
            Guid messageType1Pk = Guid.Parse(messageType1Id);
            Guid messageType2Pk = Guid.Parse(messageType2Id);

            var subscriptions = new Subscription[]
            {
                new Subscription() { Client = new Client() { __PrimaryKey = client1Pk }, MessageType = new MessageType() { __PrimaryKey = messageType1Pk } }
            };
            var subscriptionsResponse = new Subscription[]
            {
                new Subscription() { Client = new Client() { __PrimaryKey = client2Pk }, MessageType = new MessageType() { __PrimaryKey = messageType2Pk } }
            };
            var subscribtionManagerMock = new Mock<ISubscriptionsManager>();
            var statisticsServiceMock = new Mock<IStatisticsService>();
            var loggerMock = new Mock<ILogger>();
            subscribtionManagerMock.Setup(sub => sub.GetSubscriptionsForMsgType(messageType1Id, client1Id))
                .Returns(subscriptions);
            subscribtionManagerMock.Setup(sub => sub.GetSubscriptionsForMsgType(messageType2Id, client2Id))
                .Returns(subscriptionsResponse);

            var service = new RerouterService(
                subscribtionManagerMock.Object,
                statisticsServiceMock.Object,
                loggerMock.Object);
            service.Start();

            Action<HttpListenerContext> handleRequest = (context) =>
            {
                string messageText2;
                using (var stream = context.Request.InputStream)
                using (var reader = new StreamReader(stream))
                {
                    messageText2 = reader.ReadToEnd();
                }

                var buffer = Encoding.UTF8.GetBytes(messageText2);

                context.Response.ContentType = context.Request.ContentType;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            };

            var server = new HttpServer(10);
            server.ProcessRequest += handleRequest;
            server.Start(12366);

            WebRequest request = WebRequest.Create("http://localhost:12365/");
            request.Method = "POST";
            const string postData = "<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/' " +
                                    "xmlns:wsa='http://www.w3.org/2005/08/addressing'>" +
                                    "<SOAP-ENV:Header></SOAP-ENV:Header>" +
                                    "<SOAP-ENV:Body></SOAP-ENV:Body></SOAP-ENV:Envelope>";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "messageSB";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // Act.
            WebResponse response = request.GetResponse();

            string messageText;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                messageText = reader.ReadToEnd();
            }

            XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(messageText);

            server.Dispose();
            service.Stop();

            // Assert.
            Assert.Equal(responseXml.FirstChild.FirstChild.FirstChild.InnerText, config.Recievers[0].RerouteTo);
            statisticsServiceMock.Verify(stat => stat.NotifyMessageSent(subscriptions.First()), Times.Once);
            statisticsServiceMock.Verify(stat => stat.NotifyMessageReceived(subscriptions.First()), Times.Once);
            statisticsServiceMock.Verify(stat => stat.NotifyMessageSent(subscriptionsResponse.First()), Times.Once);
            statisticsServiceMock.Verify(stat => stat.NotifyMessageReceived(subscriptionsResponse.First()), Times.Once);
        }

        /// <summary>
        /// Testing ReturnWsdl method.
        /// </summary>
        [Fact]
        public void TestReturnWsdl()
        {
            // Arrange.
            var subscribtionManagerMock = new Mock<ISubscriptionsManager>();
            var statisticsServiceMock = new Mock<IStatisticsService>();
            var loggerMock = new Mock<ILogger>();

            var service = new RerouterService(
                subscribtionManagerMock.Object,
                statisticsServiceMock.Object,
                loggerMock.Object);
            service.Start();

            WebRequest request = WebRequest.Create("http://localhost:12365/?wsdl");
            request.Method = "GET";

            // Act.
            WebResponse response = request.GetResponse();

            string messageText;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                messageText = reader.ReadToEnd();
            }

            service.Stop();

            // Assert.
            Assert.Equal(messageText, "<definitions>Test WSDL text</definitions>");
        }
    }
}
