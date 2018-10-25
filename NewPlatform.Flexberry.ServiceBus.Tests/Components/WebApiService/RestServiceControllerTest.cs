namespace NewPlatform.Flexberry.ServiceBus.Tests.Components
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;
    using Flexberry.ServiceBus.Components;
    using Moq;
    using Xunit;

    public class WebApiServiceFixture : BaseServiceBusTest, IDisposable
    {
        private readonly WcfService service;

        public const string BaseAddress = "http://localhost:12347/RestServiceController";
        public Mock<ISendingManager> SendManager;
        public Mock<IReceivingManager> RecManager;

        public WebApiServiceFixture()
        {
            // Arrange.
            SendManager = new Mock<ISendingManager>();
            RecManager = new Mock<IReceivingManager>();

            var service = new WebApiService(BaseAddress, SendManager.Object, RecManager.Object);
            service.Start();
        }

        public void Dispose()
        {
            if (service != null)
            {
                service.Stop();
                service.Dispose();
            }
        }
    }

    [Collection("WebAPITests")]
    public class RestServiceControllerTest : IClassFixture<WebApiServiceFixture>
    {
        private readonly WebApiServiceFixture fixture;

        public RestServiceControllerTest(WebApiServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        /// <summary>
        /// Testing GetMessages method.
        /// </summary>
        [Fact]
        public void TestGetMessages()
        {
            // Arrange.
            const string clientId = "63C57DEC-6DA5-4B73-9156-88361D7623B4";
            const string message1Id = "Сообщение1";
            const string message2Id = "Сообщение2";
            const string messageType1Id = "Тип1";
            const string messageType2Id = "Тип2";

            DateTime messageTime = DateTime.Now;
            WebRequest request = WebRequest.Create($"{WebApiServiceFixture.BaseAddress}/Messages?clientId={clientId}");
            request.Method = "GET";
            fixture.SendManager.Setup(send => send.GetMessagesInfo(clientId, It.IsAny<int>())).Returns(new[]
            {
                new ServiceBusMessageInfo { ID = message1Id, FormingTime = messageTime, Priority = 1, MessageTypeID = messageType1Id },
                new ServiceBusMessageInfo { ID = message2Id, FormingTime = messageTime, Priority = 2, MessageTypeID = messageType2Id }
            });

            // Act.
            WebResponse response = request.GetResponse();

            ServiceBusMessageInfo[] res;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var msg = reader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                res = serializer.Deserialize<ServiceBusMessageInfo[]>(msg);
            }

            // Assert.
            fixture.SendManager.Verify(send => send.GetMessagesInfo(clientId, It.IsAny<int>()), Times.Once);
            Assert.True(res[0].ID == message1Id && res[1].ID == message2Id);
            Assert.True(res[0].Priority == 1 && res[1].Priority == 2);
            Assert.True(res[0].MessageTypeID == messageType1Id && res[1].MessageTypeID == messageType2Id);
            Assert.True(res.All(r => r.FormingTime == messageTime));
        }

        /// <summary>
        /// Testing GetMessage method.
        /// </summary>
        [Fact]
        public void TestGetMessage()
        {
            // Arrange.
            const string clientId = "B9A57043-0138-499C-965F-B802A8E499AA";
            const string messageId = "45F0E84E-0565-4F5E-A5EB-BFACF39385F1";
            const string messageTypeId = "BE32FA7E-4EE0-4EDC-9195-E914C132C522";
            const string messageBody = "Тестовый текст";
            const string sender = "Вася";

            WebRequest request = WebRequest.Create($"{WebApiServiceFixture.BaseAddress}/Message?clientId={clientId}&messageTypeId={messageTypeId}&index=0");
            request.Method = "GET";
            fixture.SendManager.Setup(send => send.ReadMessage(clientId, messageTypeId, 0))
                .Returns(new Message()
                {
                    __PrimaryKey = Guid.Parse(messageId),
                    MessageType = new MessageType() { ID = messageTypeId },
                    ReceivingTime = DateTime.Now,
                    Body = messageBody,
                    Sender = sender
                });

            // Act.
            WebResponse response = request.GetResponse();

            string msg;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                msg = reader.ReadToEnd();
            }

            // Assert.
            fixture.SendManager.Verify(send => send.ReadMessage(clientId, messageTypeId, 0), Times.Once);
            Assert.Equal(Guid.Parse(GetJsonProp(msg, "Id")), Guid.Parse(messageId));
            Assert.Equal(GetJsonProp(msg, "MessageTypeID"), messageTypeId);
            Assert.Equal(GetJsonProp(msg, "Body"), messageBody);
            Assert.Equal(GetJsonProp(msg, "SenderName"), sender);
            Assert.Equal(GetJsonProp(GetJsonProp(msg, "Tags"), "sendingWay"), ConfigurationManager.AppSettings.Get("ServiceID4SB"));
        }

        /// <summary>
        /// Testing GetMessage with id method.
        /// </summary>
        [Fact]
        public void TestGetMessageWithId()
        {
            // Arrange.
            const string messageId = "97C9A0E6-F92A-4D9E-8E9C-230D7B927522";
            const string messageTypeId = "1F3944D8-D0F5-48F4-ACD0-8F3A758D0DCA";
            const string messageBody = "12345";
            const string sender = "Паша";

            WebRequest request = WebRequest.Create($"{WebApiServiceFixture.BaseAddress}/Message/{messageId}");
            request.Method = "GET";
            fixture.SendManager.Setup(send => send.ReadMessage(messageId))
                .Returns(new Message()
                {
                    __PrimaryKey = Guid.Parse(messageId),
                    MessageType = new MessageType() { ID = messageTypeId },
                    ReceivingTime = DateTime.Now,
                    Body = messageBody,
                    Sender = sender
                });

            // Act.
            WebResponse response = request.GetResponse();

            string msg;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                 msg = reader.ReadToEnd();
            }

            // Assert.
            fixture.SendManager.Verify(send => send.ReadMessage(messageId), Times.Once);
            Assert.Equal(Guid.Parse(GetJsonProp(msg, "Id")), Guid.Parse(messageId));
            Assert.Equal(GetJsonProp(msg, "MessageTypeID"), messageTypeId);
            Assert.Equal(GetJsonProp(msg, "Body"), messageBody);
            Assert.Equal(GetJsonProp(msg, "SenderName"), sender);
            Assert.Equal(GetJsonProp(GetJsonProp(msg, "Tags"), "sendingWay"), ConfigurationManager.AppSettings.Get("ServiceID4SB"));
        }

        /// <summary>
        /// Testing PostMessage method.
        /// </summary>
        [Fact]
        public void TestPostMessage()
        {
            // Arrange.
            const string clientId = "B6AF9E87-2B78-484C-8594-B6A2659561A2";
            const string messageTypeId = "143866EF-0167-4528-B147-FA81278698AA";
            WebRequest request = WebRequest.Create($"{WebApiServiceFixture.BaseAddress}/Message");
            request.Method = "POST";
            request.ContentType = "application/json";
            var message = new MessageForESB
            {
                ClientID = clientId,
                MessageTypeID = messageTypeId
            };

            var json = new JavaScriptSerializer().Serialize(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            request.GetRequestStream().Write(buffer, 0, buffer.Length);

            // Act.
            request.GetResponse();

            // Assert.
            fixture.RecManager.Verify(rec => rec.AcceptMessage(It.Is<ServiceBusMessage>(msg => msg.ClientID == clientId && msg.MessageTypeID == messageTypeId)), Times.Once);
        }

        /// <summary>
        /// Testing DeleteMessage method.
        /// </summary>
        [Fact]
        public void TestDeleteMessage()
        {
            // Arrange.
            const string delMessageId = "2064B536-8FB3-4CBF-9FFE-B0B6FDEBDBD8";
            WebRequest request = WebRequest.Create($"{WebApiServiceFixture.BaseAddress}/Message/{delMessageId}");
            request.Method = "DELETE";

            // Act.
            request.GetResponse();

            // Assert.
            fixture.SendManager.Verify(send => send.DeleteMessage(delMessageId), Times.Once);
        }

        /// <summary>
        /// Gets json property value (not working with arrays).
        /// </summary>
        /// <param name="json">Json string.</param>
        /// <param name="propName">Property name.</param>
        /// <returns>Value of the selected property.</returns>
        private string GetJsonProp(string json, string propName)
        {
            string pattern = "(?<=\"" + propName + "\":)[^,]*(?=[,}])";
            string res = Regex.Match(json, pattern).Value;
            if (res.StartsWith("\"") && res.EndsWith("\""))
            {
                res = res.Substring(1, res.Length - 2);
            }
            else if (!res.StartsWith("{") && !res.EndsWith("}"))
            {
                res = null;
            }

            return res;
        }
    }
}
