namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Performance
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using ICSSoft.STORMNET;
    using Microsoft.Owin.Hosting;
    using Moq;
    using NewPlatform.Flexberry.ServiceBus.Components;
    using Owin;
    using Xunit;

    /// <summary>
    /// ServiceBus performance tests.
    /// </summary>
    public class ServiceBusPerformanceTest : BaseServiceBusIntegratedTest
    {
        public ServiceBusPerformanceTest()
            : base("SBPerf") { }

        private class AsyncRecieverJob
        {
            public AsyncRecieverJob(MessageForESB message, int repeatCount)
            {
                Message = message;
                RepeatCount = repeatCount;
            }

            public MessageForESB Message { get; private set; }

            public int RepeatCount { get; private set; }
        }

        private class AsyncReciever
        {
            private AsyncReciever(IReceivingManager recievingManager)
            {
                RecievingManager = recievingManager;
            }

            public AsyncReciever(IReceivingManager recievingManager, IEnumerable<AsyncRecieverJob> jobs) : this(recievingManager)
            {
                Jobs = jobs;
            }

            public AsyncReciever(IReceivingManager recievingManager, IEnumerable<MessageForESB> messages) : this(recievingManager)
            {
                Jobs = messages
                    .Select(x => new AsyncRecieverJob(x, 1))
                    .ToArray();
            }

            public double RecievingTime { get; private set; }

            public bool StopFlag { get; private set; }

            public void SetStopFlag()
            {
                StopFlag = true;
            }

            public void AsyncRecieve()
            {
                var sw = Stopwatch.StartNew();
                foreach (var job in Jobs)
                {
                    if (StopFlag)
                    {
                        break;
                    }

                    int repeatCount = job.RepeatCount;

                    for (int i = 0; i < repeatCount; i++)
                    {
                        if (StopFlag)
                        {
                            break;
                        }

                        RecievingManager.AcceptMessage(job.Message);
                    }
                }

                sw.Stop();
                RecievingTime = sw.Elapsed.TotalMilliseconds;
            }

            private IEnumerable<AsyncRecieverJob> Jobs { get; }

            private IReceivingManager RecievingManager { get; }
        }

        [Fact(Skip = "Only manual start.")]
        public void TestReciveSendMessageBlockWcf()
        {
            const string BaseAddress = "http://localhost:2525/Message";
            const string MessgeIdPattern = "(MessageId>urn:uuid:)([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})(</)";

            const int BlockSize = 1000;
            const int AttachmentSize = 1024;

            const int SendTimeout = 2000;

            foreach (var dataService in DataServices)
            {
                // Arrange.
                Exception exception = null;

                var sender = new Client() { ID = "senderId" };
                var recipient = new Client() { ID = "recipientId", Address = BaseAddress };
                var messageType = new MessageType() { ID = "messageTypeId" };
                var subscription = new Subscription()
                {
                    IsCallback = true,
                    Client = recipient,
                    MessageType = messageType,
                    TransportType = TransportType.WCF,
                    ExpiryDate = DateTime.Now.AddDays(1),
                };

                var dataObjects = new DataObject[] { sender, recipient, messageType, subscription };
                dataService.UpdateObjects(ref dataObjects);

                var statisticsService = GetMockStatisticsService();
                var subscriptionManager = new DefaultSubscriptionsManager(dataService, statisticsService);
                var sendingManager = new OptimizedSendingManager(
                    subscriptionManager,
                    statisticsService,
                    dataService,
                    GetMockLogger());

                var mockObjectRepository = new Mock<IObjectRepository>();
                mockObjectRepository
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender, MessageType = messageType } });
                var recievingManager = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    subscriptionManager,
                    sendingManager,
                    dataService,
                    statisticsService);

                var msg = new MessageForESB()
                {
                    ClientID = sender.ID,
                    MessageTypeID = messageType.ID,
                    Body = $"{AttachmentSize.ToString()} bytes attachment.",
                    Tags = new Dictionary<string, string>() { { "senderName", sender.ID } },
                    Attachment = new byte[AttachmentSize],
                };

                AsyncRecieverJob[] asyncJobs = { new AsyncRecieverJob(msg, BlockSize) };

                var asyncReciever = new AsyncReciever(recievingManager, asyncJobs);

                double sendingTime = 0;

                // Act.
                var sendedMessages = new Dictionary<Guid, object>();

                using (AutoResetEvent resetEvent = new AutoResetEvent(false))
                using (WebApp.Start(BaseAddress, builder =>
                {
                    builder.Run(context =>
                    {
                        try
                        {
                            var requestBody = new StreamReader(context.Request.Body).ReadToEnd();
                            var match = Regex.Match(requestBody, MessgeIdPattern, RegexOptions.IgnoreCase);
                            Guid messageId = Guid.Parse(match.Groups[2].Value);
                            try
                            {
                                sendedMessages.Add(messageId, null);
                            }
                            catch (ArgumentException)
                            {
                                throw new Exception($"{dataService.ToString()}. Сообщение (MessageID: {messageId.ToString("D")}) отправлено повторно.");
                            }
                        }
                        catch (Exception ex)
                        {
                            exception = exception ?? ex;
                        }
                        resetEvent.Set();

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
                    int timeout = sendingManager.ScanningPeriodMilliseconds + SendTimeout;

                    var recievingThread = new Thread(asyncReciever.AsyncRecieve);

                    sendingManager.Prepare();
                    sendingManager.Start();

                    try
                    {
                        var sw = new Stopwatch();

                        bool sendingStarted = false;
                        int n = 0;

                        recievingThread.Start();

                        while (true)
                        {
                            var now = DateTime.Now;

                            // wait for the callback
                            resetEvent.WaitOne(timeout);

                            // synchronous waiting time measurement
                            double waitingTime = (DateTime.Now - now).TotalMilliseconds;

                            if (!sendingStarted)
                            {
                                timeout = SendTimeout;
                                sendingStarted = true;
                                sw.Start();
                            }

                            if (exception != null)
                            {
                                throw exception;
                            }

                            var sendedCount = sendedMessages.Count;

                            if (sendedCount > BlockSize)
                            {
                                // the test will fail at Assert block
                                break;
                            }

                            if (sendedCount == n)
                            {
                                if (waitingTime < SendTimeout)
                                {
                                    continue;
                                }

                                if (sendedCount < BlockSize)
                                {
                                    throw new TimeoutException($"{dataService.ToString()}. Время ожидания callback-вызова превысило {SendTimeout} мсек.");
                                }

                                // the test result will be determined at Assert block
                                break;
                            }

                            n = sendedCount;
                        }

                        sw.Stop();
                        sendingTime = sw.Elapsed.TotalMilliseconds - SendTimeout;
                    }
                    catch (Exception ex)
                    {
                        asyncReciever.SetStopFlag();
                        exception = exception ?? ex;
                    }
                    finally
                    {
                        sendingManager.Stop();
                        sendingManager.AfterStop();
                    }

                    recievingThread.Join();

                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                // Assert.
                Assert.False(sendedMessages.Count > BlockSize, $"{ dataService.ToString()}. Количество отправленных сообщений превысило количество полученных сообщений.");

                var diagnosticMessage = $"{dataService.ToString()}:";
                diagnosticMessage += $"{Environment.NewLine}  Recieving time {asyncReciever.RecievingTime.ToString()} ms.";
                diagnosticMessage += $"{Environment.NewLine}  Sending time {sendingTime.ToString()} ms.";
                Console.WriteLine(diagnosticMessage);
            }
        }
    }
}
