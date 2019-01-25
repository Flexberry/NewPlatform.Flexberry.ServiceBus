namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using Microsoft.Owin.Hosting;
    using Owin;
    using RazorEngine;
    using Xunit;

    public class DefaultSendingManagerTest : BaseServiceBusIntegratedTest
    {
        public DefaultSendingManagerTest()
            : base("testDSM")
        {
        }

        [Fact]
        public void TestJustSendMessageByHttp()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var requestBody = string.Empty;
                var message = InitTestData("http://localhost:2525", TransportType.HTTP, dataService);
                var statisticsService = GetMockStatisticsService();
                var subscriptionsManager = new DefaultSubscriptionsManager(dataService, statisticsService);
                var component = new DefaultSendingManager(
                    subscriptionsManager,
                    statisticsService,
                    dataService,
                    GetMockLogger());

                using (WebApp.Start("http://localhost:2525/Message", (builder) =>
                {
                    builder.Run((context) =>
                    {
                        requestBody = new StreamReader(context.Request.Body).ReadToEnd();
                        return context.Response.WriteAsync(string.Empty);
                    });
                }))
                {
                    // Act.
                    Act(component);

                    // Assert.
                    Assert.True(ValidateRequest(requestBody, message, TransportType.HTTP));
                    Assert.Equal(0, dataService.GetObjectsCount(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageListView)));
                }
            }
        }

        [Fact]
        public void TestJustSendMessageByWeb()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var requestBody = string.Empty;
                var message = InitTestData("http://localhost:2525", TransportType.WEB, dataService);
                var statisticsService = GetMockStatisticsService();
                var subscriptionsManager = new DefaultSubscriptionsManager(dataService, statisticsService);
                var component = new DefaultSendingManager(
                    subscriptionsManager,
                    statisticsService,
                    dataService,
                    GetMockLogger());

                using (WebApp.Start("http://localhost:2525/Message", builder =>
                {
                    builder.Run(context =>
                    {
                        requestBody = new StreamReader(context.Request.Body).ReadToEnd();
                        context.Response.ContentType = "text/xml; charset=utf-8";
                        return context.Response.WriteAsync(
                            @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
                                <s:Header />
                                <s:Body>
                                    <AcceptMessageResponse xmlns=""http://tempuri.org/"" />
                                </s:Body>
                            </s:Envelope>");
                    });
                }))
                {
                    // Act.
                    Act(component);

                    // Assert.
                    Assert.True(ValidateRequest(requestBody, message, TransportType.WEB));
                    Assert.Equal(0, dataService.GetObjectsCount(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageListView)));
                }
            }
        }

        [Fact]
        public void TestJustSendMessageByWcf()
        {
            foreach (var dataService in DataServices)
            {
                // Arrange.
                var requestBody = string.Empty;
                var message = InitTestData("http://localhost:2525", TransportType.WCF, dataService);
                var statisticsService = GetMockStatisticsService();
                var subscriptionsManager = new DefaultSubscriptionsManager(dataService, statisticsService);
                var component = new DefaultSendingManager(
                    subscriptionsManager,
                    statisticsService,
                    dataService,
                    GetMockLogger());

                using (WebApp.Start("http://localhost:2525/Message", builder =>
                {
                    builder.Run(context =>
                    {
                        requestBody = new StreamReader(context.Request.Body).ReadToEnd();
                        context.Response.ContentType = "application/soap+xml; charset=utf-8";
                        return context.Response.WriteAsync(
                            @"<s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
                                <s:Header />
                                <s:Body>
                                    <AcceptMessageResponse xmlns=""http://tempuri.org/"" />
                                </s:Body>
                            </s:Envelope>");
                    });
                }))
                {
                    // Act.
                    Act(component);

                    // Assert.
                    Assert.True(ValidateRequest(requestBody, message, TransportType.WCF));
                    Assert.Equal(0, dataService.GetObjectsCount(LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.MessageListView)));
                }
            }
        }

        private void Act(DefaultSendingManager component)
        {
            component.Prepare();
            component.Start();
            Thread.Sleep(component.ScanningPeriodMilliseconds);
            component.Stop();
            component.AfterStop();
        }

        private Message InitTestData(string baseAddress, TransportType transportType, IDataService dataService)
        {
            if (transportType != TransportType.HTTP)
                baseAddress += "/Message";

            var messageType = new MessageType() { ID = "messageTypeId" };
            var recipient = new Client() { ID = "recipientId", Address = baseAddress };
            var message = new Message()
            {
                Body = "BodyBum!",
                Group = "group",
                Recipient = recipient,
                Sender = "senderId",
                MessageType = messageType,
                Tags = "Color:Black;Name:Jack;",
                ReceivingTime = DateTime.Now,
                BinaryAttachment = new byte[] { 72, 101, 108, 108, 111, 44, 32, 119, 111, 114, 108, 100, 33 },
            };
            var dataObjects = new DataObject[]
            {
                message,
                recipient,
                messageType,
                new Subscription()
                {
                    IsCallback = true,
                    Client = recipient,
                    MessageType = messageType,
                    TransportType = transportType,
                    ExpiryDate = DateTime.Now.AddDays(1),
                },
            };
            dataService.UpdateObjects(ref dataObjects);
            return message;
        }

        private bool ValidateRequest(string request, Message message, TransportType transportType)
        {
            string requestTemplate;
            string folder = @"Components\SendingManager\RequestTemplates\";
            switch (transportType)
            {
                case TransportType.WCF:
                    requestTemplate = File.ReadAllText(folder + "WCFRequestTemplate.txt");
                    break;
                case TransportType.WEB:
                    requestTemplate = File.ReadAllText(folder + "WebRequestTemplate.txt");
                    break;
                case TransportType.HTTP:
                    requestTemplate = File.ReadAllText(folder + "HTTPRequestTemplate.txt");
                    break;
                default:
                    throw new ArgumentException("Invalid value.", nameof(transportType));
            }

            return Regex.IsMatch(request, Razor.Parse(requestTemplate, message).Replace("/", @"\/").Replace(".", @"\."));
        }
    }
}