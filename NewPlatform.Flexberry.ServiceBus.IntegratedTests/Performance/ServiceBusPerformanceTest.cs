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
            const string BaseAddressRecipient1 = "http://localhost:2525/Message";
            const string BaseAddressRecipient2 = "http://localhost:2526/Message";
            const string BaseAddressRecipient3 = "http://localhost:2527/Message";
            const string MessgeIdPattern = "(MessageId>urn:uuid:)([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})(</)";

            const int BlockSize1 = 1000;
            const int BlockSize2 = 100;
            const int BlockSizeAll = 1200;
            const int AttachmentSize = 1024;

            const int SendTimeout = 2000;

            foreach (var dataService in DataServices)
            {
                // Arrange.
                Exception exception = null;

                var sender1 = new Client() { ID = "senderId1" };
                var recipient1 = new Client() { ID = "recipientId1", Address = BaseAddressRecipient1 };
                var messageType1 = new MessageType() { ID = "messageTypeId1" };
                var subscription1 = new Subscription()
                {
                    IsCallback = true,
                    Client = recipient1,
                    MessageType = messageType1,
                    TransportType = TransportType.WCF,
                    ExpiryDate = DateTime.Now.AddDays(1),
                };

                var sender2 = new Client() { ID = "senderId2" };
                var recipient2 = new Client() { ID = "recipientId2", Address = BaseAddressRecipient2 };
                var messageType2 = new MessageType() { ID = "messageTypeId2" };
                var subscription2 = new Subscription()
                {
                    IsCallback = true,
                    Client = recipient2,
                    MessageType = messageType2,
                    TransportType = TransportType.WCF,
                    ExpiryDate = DateTime.Now.AddDays(1),
                };

                var sender3 = new Client() { ID = "senderId3" };
                var recipient3 = new Client() { ID = "recipientId3", Address = BaseAddressRecipient3 };
                var messageType3 = new MessageType() { ID = "messageTypeId3" };
                var subscription3 = new Subscription()
                {
                    IsCallback = true,
                    Client = recipient3,
                    MessageType = messageType3,
                    TransportType = TransportType.WCF,
                    ExpiryDate = DateTime.Now.AddDays(1),
                };
                var dataObjects = new DataObject[] 
                {
                    sender1, recipient1, messageType1, subscription1,
                    sender2, recipient2, messageType2, subscription2,
                    sender3, recipient3, messageType3, subscription3,
                };
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
                    .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender1.ID)))
                    .Returns(new[] { new SendingPermission() { Client = sender1, MessageType = messageType1 } });
                mockObjectRepository
                   .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender2.ID)))
                   .Returns(new[] { new SendingPermission() { Client = sender2, MessageType = messageType2 } });
                mockObjectRepository
                   .Setup(or => or.GetRestrictionsForClient(It.Is<string>(id => id == sender3.ID)))
                   .Returns(new[] { new SendingPermission() { Client = sender3, MessageType = messageType3 } });

                var recievingManager = new DefaultReceivingManager(
                    GetMockLogger(),
                    mockObjectRepository.Object,
                    subscriptionManager,
                    sendingManager,
                    dataService,
                    statisticsService);

                // Messages of 1 kb (100 pieces).
                var msg1 = new MessageForESB()
                {
                    ClientID = sender1.ID,
                    MessageTypeID = messageType1.ID,
                    Body = $"{AttachmentSize.ToString()} bytes attachment.",
                    Tags = new Dictionary<string, string>() { { "senderName", sender1.ID } },
                    Attachment = new byte[AttachmentSize],
                };

                AsyncRecieverJob[] asyncJobs1 = { new AsyncRecieverJob(msg1, 1) };
                var asyncReciever1 = new AsyncReciever(recievingManager, asyncJobs1);

                // Messages of 1 kb (1000 pieces).
                var msg2 = new MessageForESB()
                {
                    ClientID = sender2.ID,
                    MessageTypeID = messageType2.ID,
                    Body = $"{AttachmentSize.ToString()} bytes attachment.",
                    Tags = new Dictionary<string, string>() { { "senderName", sender2.ID } },
                    Attachment = new byte[AttachmentSize],
                };

                AsyncRecieverJob[] asyncJobs2 = { new AsyncRecieverJob(msg2, BlockSize1) };
                var asyncReciever2 = new AsyncReciever(recievingManager, asyncJobs2);

                // Messages of 1 mb (100 pieces).
                var msg3 = new MessageForESB()
                {
                    ClientID = sender3.ID,
                    MessageTypeID = messageType3.ID,
                    Body = $"{AttachmentSize.ToString()} Kilobyte attachment.",
                    Tags = new Dictionary<string, string>() { { "senderName", sender3.ID } },
                    Attachment = new byte[AttachmentSize * AttachmentSize],
                };

                AsyncRecieverJob[] asyncJobs3 = { new AsyncRecieverJob(msg3, BlockSize2) };
                var asyncReciever3 = new AsyncReciever(recievingManager, asyncJobs3);

                double sendingTime = 0;

                // Act.
                var sendedMessages = new Dictionary<Guid, object>();

                using (AutoResetEvent resetEvent = new AutoResetEvent(false))
                using (WebApp.Start(BaseAddressRecipient1, builder =>
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
                using (WebApp.Start(BaseAddressRecipient2, builder =>
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
                using (WebApp.Start(BaseAddressRecipient3, builder =>
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

                    var recievingThread1 = new List<Thread>();
                    for (int i = 0; i < BlockSize2; i++)
                    {
                        recievingThread1.Add(new Thread(asyncReciever1.AsyncRecieve));
                    }

                    var recievingThread2 = new Thread(asyncReciever2.AsyncRecieve);
                    var recievingThread3 = new Thread(asyncReciever3.AsyncRecieve);

                    sendingManager.Prepare();
                    sendingManager.Start();

                    try
                    {
                        var sw = new Stopwatch();

                        bool sendingStarted = false;
                        int n = 0;

                        for (int i = 0; i < BlockSize2; i++)
                        {
                            recievingThread1[i].Start();
                            Thread.Sleep(1000);
                        }

                        recievingThread2.Start();
                        recievingThread3.Start();

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

                            if (sendedCount > BlockSizeAll)
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

                                if (sendedCount < BlockSizeAll)
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
                        asyncReciever1.SetStopFlag();
                        asyncReciever2.SetStopFlag();
                        asyncReciever3.SetStopFlag();
                        exception = exception ?? ex;
                    }
                    finally
                    {
                        sendingManager.Stop();
                        sendingManager.AfterStop();
                    }

                    for (int i = 0; i < BlockSize2; i++)
                    {
                        recievingThread1[i].Join();
                    }

                    recievingThread2.Join();
                    recievingThread3.Join();

                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                // Assert.
                Assert.False(sendedMessages.Count > BlockSizeAll, $"{ dataService.ToString()}. Количество отправленных сообщений превысило количество полученных сообщений.");

                var diagnosticMessage = $"{dataService.ToString()}:";
                diagnosticMessage += $"{Environment.NewLine}  Recieving time 100 msg by 1kb {asyncReciever1.RecievingTime.ToString()} ms.";
                diagnosticMessage += $"{Environment.NewLine}  Recieving time 1000 msg by 1kb {asyncReciever2.RecievingTime.ToString()} ms.";
                diagnosticMessage += $"{Environment.NewLine}  Recieving time 100 msg by 1 mb {asyncReciever3.RecievingTime.ToString()} ms.";
                diagnosticMessage += $"{Environment.NewLine}  Sending time {sendingTime.ToString()} ms.";
                Console.WriteLine(diagnosticMessage);
            }
        }
    }
}
