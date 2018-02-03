namespace NewPlatform.Flexberry.ServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Flexberry.ServiceBus.Components;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for methods of ServieHelper class.
    /// </summary>
    public class ServiceHelperTest : BaseServiceBusTest
    {
        /// <summary>
        /// Test for GetIntConfigParam method.
        /// </summary>
        [Fact]
        public void TestGetIntConfigParam()
        {
            // Arrange.
            var logger = new Mock<ILogger>();
            const int defaultResult = 10;

            // Act & Assert.
            var result = ServiceHelper.GetIntConfigParam("WrongIntParamGetTest", defaultResult, logger.Object);
            Assert.Equal(defaultResult, result);
            logger.Verify(log => log.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Message>()), Times.Once);

            result = ServiceHelper.GetIntConfigParam("IntParamGetTest", defaultResult, logger.Object);
            Assert.Equal(100, result);
        }

        /// <summary>
        /// Test for CreateWcfMessageFromEsb method.
        /// </summary>
        [Fact]
        public void TestCreateWcfMessageFromEsb()
        {
            // Arrange.
            var formTime = DateTime.Now;
            const string messageTypeId = "03FE3B98-2D09-4032-A5BF-03BEDF86F4F4";
            const string msgBody = "TestBody";
            const string senderName = "Sender's name";
            const string groupId = "715A8124-A154-485B-83AC-6EE6BA7A9470";
            Dictionary<string, string> tags = new Dictionary<string, string> { { "testTag", "tag" } };
            byte[] attachment = Encoding.Unicode.GetBytes(msgBody);

            // Act.
            MessageFromESB msg = ServiceHelper.CreateWcfMessageFromEsb(formTime, messageTypeId, msgBody, senderName, groupId, tags, attachment);

            // Assert.
            Assert.True(msg.MessageFormingTime == formTime && msg.MessageTypeID == messageTypeId && msg.Body == msgBody &&
                        msg.SenderName == senderName && msg.GroupID == groupId && msg.Tags == tags &&
                        msg.Attachment == attachment);
        }

        /// <summary>
        /// Test for CreateHttpMessageFromEsb method.
        /// </summary>
        [Fact]
        public void TestCreateHttpMessageFromEsb()
        {
            // Arrange.
            var formTime = DateTime.Now;
            const string id = "79FE15AA-4EEB-4337-9EDC-1B87577724C6";
            const string messageTypeId = "03FE3B98-2D09-4032-A5BF-03BEDF86F4F4";
            const string msgBody = "TestBody";
            const string senderName = "Sender's name";
            const string groupId = "715A8124-A154-485B-83AC-6EE6BA7A9470";
            Dictionary<string, string> tags = new Dictionary<string, string> { { "testTag", "tag" } };
            byte[] attachment = Encoding.Unicode.GetBytes(msgBody);

            // Act.
            HttpMessageFromEsb msg = ServiceHelper.CreateHttpMessageFromEsb(id, formTime, messageTypeId, msgBody, senderName, groupId, tags, attachment);

            // Assert.
            Assert.True(msg.Id == id && msg.MessageFormingTime == formTime && msg.MessageTypeID == messageTypeId &&
                        msg.Body == msgBody && msg.SenderName == senderName && msg.GroupID == groupId &&
                        msg.Tags == tags && msg.Attachment == attachment);
        }

        /// <summary>
        /// Test for GetTagDictionary method.
        /// </summary>
        [Fact]
        public void TestGetTagDictionary()
        {
            // Arrange.
            const string noTags = "There is not a single tag...";
            const string simpleTags = "Color:Red;Language:English;";
            const string withEmptyTags = "Color:;Language:English;";
            const string duplicateTags = "Color:Red;Language:English;Language:Russian;";
            var noTagsDictionary = new Dictionary<string, string>
            {
                { "There is not a single tag...", "There is not a single tag..." }
            };
            var simpleTagsDictionary = new Dictionary<string, string> { { "Color", "Red" }, { "Language", "English" } };
            var withEmptyTagsDictionary = new Dictionary<string, string>
            {
                { "Color", string.Empty },
                { "Language", "English" }
            };

            // Act & Assert.
            Assert.Equal(0, ServiceHelper.GetTagDictionary(new Message() { Tags = null }).Count);
            Assert.Equal(noTagsDictionary, ServiceHelper.GetTagDictionary(new Message() { Tags = noTags }));
            Assert.Equal(simpleTagsDictionary, ServiceHelper.GetTagDictionary(new Message() { Tags = simpleTags }));
            Assert.Equal(withEmptyTagsDictionary, ServiceHelper.GetTagDictionary(new Message() { Tags = withEmptyTags }));
            Assert.Throws<ArgumentException>(() => ServiceHelper.GetTagDictionary(new Message() { Tags = duplicateTags }));
        }

        /// <summary>
        /// Test for ConvertClientIdToPrimaryKey method.
        /// </summary>
        [Fact]
        public void TestConvertClientIdToPrimaryKey()
        {
            // Arrange.
            const string id = "79FE15AA-4EEB-4337-9EDC-1B87577724C6";
            const string id2 = "715A8124-A154-485B-83AC-6EE6BA7A9470";
            var dataServiceMock = new Mock<IDataService>();
            dataServiceMock.Setup(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(Client)))).Returns(new DataObject[] { new Client() { __PrimaryKey = id } });
            var dataServiceMock2 = new Mock<IDataService>();
            dataServiceMock2.Setup(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(Client)))).Returns(new DataObject[] { });

            // Act & Assert.
            Assert.Equal(Guid.Parse(id), ServiceHelper.ConvertClientIdToPrimaryKey(id2, dataServiceMock.Object, GetMockStatisticsService()));
            Assert.Equal(Guid.Parse(id), ServiceHelper.ConvertClientIdToPrimaryKey(id, dataServiceMock2.Object, GetMockStatisticsService()));
            Assert.Throws<InvalidOperationException>(() => ServiceHelper.ConvertClientIdToPrimaryKey("abc", dataServiceMock2.Object, GetMockStatisticsService()));
        }

        /// <summary>
        /// Test for ConvertMessageTypeIdToPrimaryKey method.
        /// </summary>
        [Fact]
        public void TestConvertMessageTypeIdToPrimaryKey()
        {
            // Arrange.
            const string id = "79FE15AA-4EEB-4337-9EDC-1B87577724C6";
            const string id2 = "715A8124-A154-485B-83AC-6EE6BA7A9470";
            var dataServiceMock = new Mock<IDataService>();
            dataServiceMock.Setup(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(MessageType)))).Returns(new DataObject[] { new MessageType() { __PrimaryKey = id } });
            var dataServiceMock2 = new Mock<IDataService>();
            dataServiceMock2.Setup(f => f.LoadObjects(It.Is<LoadingCustomizationStruct>(lcs => lcs.View.DefineClassType == typeof(MessageType)))).Returns(new DataObject[] { });

            // Act & Assert.
            Assert.Equal(Guid.Parse(id), ServiceHelper.ConvertMessageTypeIdToPrimaryKey(id2, dataServiceMock.Object, GetMockStatisticsService()));
            Assert.Equal(Guid.Parse(id), ServiceHelper.ConvertMessageTypeIdToPrimaryKey(id, dataServiceMock2.Object, GetMockStatisticsService()));
            Assert.Throws<InvalidOperationException>(() => ServiceHelper.ConvertMessageTypeIdToPrimaryKey("abc", dataServiceMock2.Object, GetMockStatisticsService()));
        }

        /// <summary>
        /// Test for TryWithExceptionLogging method.
        /// </summary>
        [Fact]
        public void TestTryWithExceptionLogging()
        {
            // Arrange.
            var mockLogger = new Mock<ILogger>();
            var exception = new Exception("UnhandledException");
            Action withExceptionAction = () => { throw exception; };
            Action successAction = () => { Assert.True(true); };

            // Act & Assert.
            Assert.False(ServiceHelper.TryWithExceptionLogging(withExceptionAction, successAction, string.Empty, new Client(), new Message(), mockLogger.Object));
            mockLogger.Verify(log => log.LogUnhandledException(It.IsAny<Exception>(), It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Throws<ArgumentNullException>(() => ServiceHelper.TryWithExceptionLogging(null, successAction, string.Empty, new Client(), new Message(), mockLogger.Object));
            Assert.True(ServiceHelper.TryWithExceptionLogging(successAction, successAction, string.Empty, new Client(), new Message(), mockLogger.Object));
        }

        /// <summary>
        /// Test for GetMessagesLcs method.
        /// </summary>
        [Fact]
        public void TestGetMessagesLcs()
        {
            // Arrange.
            ExternalLangDef langDef = ExternalLangDef.LanguageDef;
            var limitFunction = langDef.GetFunction(
                langDef.funcNOT,
                new VariableDef(langDef.BoolType, Information.ExtractPropertyPath<Message>(x => x.IsSending)));

            // Act.
            var lcs = ServiceHelper.GetMessagesLcs(limitFunction);

            // Assert.
            Assert.True(lcs.View.DefineClassType == typeof(Message) && lcs.LimitFunction == limitFunction);
        }

        /// <summary>
        /// Test for AddSenderToMessage method.
        /// </summary>
        [Fact]
        public void TestAddSenderToMessage()
        {
            // Arrange.
            var messageForESB = new MessageForESB() { Tags = new Dictionary<string, string>() };
            var message = new Message();
            messageForESB.Tags.Add("senderName", "sender");

            // Act.
            ServiceHelper.AddSenderToMessage(messageForESB, message, null, new Mock<IDataService>().Object, new Mock<ILogger>().Object, GetMockStatisticsService());

            // Assert.
            Assert.Equal("sender", message.Sender);
        }

        /// <summary>
        /// Test for SaveTag method.
        /// </summary>
        [Fact]
        public void TestSaveTag()
        {
            // Arrange.
            var tags = new Dictionary<string, string> { { "MessageForESBTag", "MessageForESBTagValue" } };
            var messageForEsb = new MessageForESB { ClientID = "guid", Tags = tags };
            var message = new Message() { Tags = "MessageTag:TagValue;sendingWay:start" };
            var message2 = new Message() { Tags = "MessageTag:TagValue" };

            // Act.
            ServiceHelper.SaveTag(messageForEsb, message);
            ServiceHelper.SaveTag(messageForEsb, message2);

            // Assert.
            Assert.Equal("MessageTag:TagValue;sendingWay:start/guid;MessageForESBTag:MessageForESBTagValue", message.Tags);
            Assert.Equal("MessageTag:TagValue;sendingWay:guid;MessageForESBTag:MessageForESBTagValue", message2.Tags);
        }

        /// <summary>
        /// Test for UpdateStoppingDate method.
        /// </summary>
        [Fact]
        public void TestUpdateStoppingDate()
        {
            // Arrange.
            var subscription = new Subscription();

            // Act.
            ServiceHelper.UpdateStoppingDate(subscription);

            // Assert.
            Assert.True(DateTime.Now < subscription.ExpiryDate);
        }

        /// <summary>
        /// Test for UpdateObject method.
        /// </summary>
        [Fact]
        public void TestUpdateObject()
        {
            // Arrange.
            var client = new Client();
            var logger = new Mock<ILogger>();
            var okDataService = new Mock<IDataService>();
            var unhandledDataService = new Mock<IDataService>();
            var unhandledFlag = false;
            unhandledDataService.Setup(f => f.UpdateObject(It.IsAny<DataObject>())).Callback(() =>
            {
                if (!unhandledFlag)
                {
                    unhandledFlag = true;
                    throw new ExecutingQueryException("Test", "Test", new NullReferenceException());
                }
            });

            // Act.
            ServiceHelper.UpdateObject(okDataService.Object, client, GetMockLogger(), GetMockStatisticsService());
            ServiceHelper.UpdateObject(unhandledDataService.Object, client, logger.Object, GetMockStatisticsService());

            // Assert.
            okDataService.Verify(f => f.UpdateObject(It.IsAny<Client>()), Times.Once);
            unhandledDataService.Verify(f => f.UpdateObject(It.IsAny<Client>()), Times.Exactly(2));
            logger.Verify(log => log.LogUnhandledException(It.IsAny<NullReferenceException>(), It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Test for UpdateObjects method.
        /// </summary>
        [Fact]
        public void TestUpdateObjects()
        {
            // Arrange.
            DataObject[] clients = { new Client(), new Client() };
            DataObject[] nullClients = null;
            DataObject[] emptyClients = { };
            var logger = new Mock<ILogger>();
            var okDataService = new Mock<IDataService>();
            var unhandledDataService = new Mock<IDataService>();
            var emptyDataService = new Mock<IDataService>();
            var unhandledFlag = false;
            unhandledDataService.Setup(f => f.UpdateObjects(ref clients)).Callback(() =>
            {
                if (!unhandledFlag)
                {
                    unhandledFlag = true;
                    throw new ExecutingQueryException("Test", "Test", new NullReferenceException());
                }
            });

            // Act.
            ServiceHelper.UpdateObjects(okDataService.Object, ref clients, GetMockLogger(), GetMockStatisticsService());
            ServiceHelper.UpdateObjects(unhandledDataService.Object, ref clients, logger.Object, GetMockStatisticsService());
            ServiceHelper.UpdateObjects(emptyDataService.Object, ref emptyClients, GetMockLogger(), GetMockStatisticsService());

            // Assert.
            Assert.Throws<ArgumentNullException>(
                () => ServiceHelper.UpdateObjects(GetMockDataService(), ref nullClients, GetMockLogger(), GetMockStatisticsService()));
            emptyDataService.Verify(f => f.UpdateObjects(ref emptyClients), Times.Never);
            okDataService.Verify(f => f.UpdateObjects(ref clients), Times.Once);
            unhandledDataService.Verify(f => f.UpdateObjects(ref clients), Times.Exactly(2));
            logger.Verify(log => log.LogUnhandledException(It.IsAny<NullReferenceException>(), It.IsAny<Message>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
