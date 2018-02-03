namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml;
    using Rerouter;
    using RerouterConfiguration;

    /// <summary>
    /// Implementation of rerouter service.
    /// </summary>
    internal class RerouterService : BaseServiceBusComponent, IRerouterService
    {
        private readonly ISubscriptionsManager _subscriptionsManager;

        private readonly IStatisticsService _statisticsService;

        private readonly ILogger _logger;

        private HttpServer _server = null;

        /// <summary>
        /// Потокобезопасная коллекция для хранения контекста http-соединений в ожидании ответа из шины.
        /// </summary>
        private static ConcurrentDictionary<Guid, HttpListenerContext> contexts = new ConcurrentDictionary<Guid, HttpListenerContext>();

        /// <summary>
        /// Методы запросов, которые могут иметь тело запроса.
        /// </summary>
        private static string[] RequestsWithMessageBody = { "POST", "PUT" };

        /// <summary>
        /// Rerouter service is enabled.
        /// Enabled by default.
        /// </summary>
        public bool Enabled { get; set; } = true;

        public RerouterService(ISubscriptionsManager subscriptionsManager, IStatisticsService statisticsService, ILogger logger)
        {
            if (subscriptionsManager == null)
                throw new ArgumentNullException(nameof(subscriptionsManager));

            if (statisticsService == null)
                throw new ArgumentNullException(nameof(statisticsService));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _subscriptionsManager = subscriptionsManager;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        public override void Start()
        {
            try
            {
                var config = MessageRerouterConfiguration.Current;

                if (Enabled && config.Recievers != null)
                {
                    _server = new HttpServer(10);
                    _server.ProcessRequest += RerouteRequest;
                    _server.Start(config.ServerPort);
                }
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex, null, "При запуске сервиса переадресации произошла ошибка.");
            }
        }

        /// <summary>
        /// Остановка компонента. Сервисы и потоки останавливаются.
        /// </summary>
        public override void Stop()
        {
            if (_server != null)
                _server.Stop();
        }

        /// <summary>
        /// Обработчик входящего http-соединения.
        /// </summary>
        /// <param name="context">Контекст http-соединения.</param>
        private void RerouteRequest(HttpListenerContext context)
        {
            try
            {
                var config = MessageRerouterConfiguration.Current;

                // Поиск URL в конфиге.
                var urlConfig = config.Recievers[context.Request.Url.AbsolutePath];
                if (urlConfig == null)
                {
                    ReturnCode(context, 404);
                    return;
                }

                // Если запрашивается wsdl, вернуть wsdl.
                if (context.Request.Url.PathAndQuery.EndsWith("?wsdl"))
                {
                    ReturnWsdl(context, urlConfig.Wsdl);
                    return;
                }

                // Чтение тела сообщения.
                string messageText;
                using (var stream = context.Request.InputStream)
                using (var reader = new StreamReader(stream))
                {
                    messageText = reader.ReadToEnd();
                }

                // Работа с wsa:To подразумевает формат сообщения SOAP.
                if (urlConfig.FixWsaTo)
                {
                    // Чтение xml-документа из сообщения.
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(messageText);

                    // Фиксим wsa:To.
                    FixWsaTo(ref xmlDocument, urlConfig.RerouteTo);

                    messageText = xmlDocument.OuterXml;
                }

                // Добавление открытого контекста в пул контекстов.
                var contextGuid = Guid.NewGuid();
                contexts[contextGuid] = context;

                // Заполнение данных о сообщении.
                var messageInfo = new MessageInfo
                {
                    ContextId = contextGuid,
                    RerouteUrl = urlConfig.RerouteTo + context.Request.Url.Query,
                    SbResponseType = urlConfig.SbResponseType,
                    ContentType = context.Request.ContentType,
                    AcceptTypes = string.Join(";", context.Request.AcceptTypes ?? new string[0]),
                    HttpMethod = context.Request.HttpMethod,
                };

                // Имитация отправки сообщения через шину.
                ImitateServiceBusMessage(config.ReceiverId, config.SenderId, urlConfig.SbRequestType);

                // Обработка сообщения.
                ProcessRequest(messageInfo, messageText);
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex, null, "При перенаправлении http-запроса произошла ошибка.");
                try
                {
                    ReturnCode(context, 400);
                }
                catch { }
            }
        }

        /// <summary>
        /// Осуществляет передачу запроса конечному получателю и возвращает ответ в шину.
        /// </summary>
        /// <param name="messageInfo">Внутреннее сообщение шины.</param>
        /// <param name="message">Запрос от изначального отправителя.</param>
        private void ProcessRequest(MessageInfo messageInfo, string message)
        {
            try
            {
                var config = MessageRerouterConfiguration.Current;

                // Создание запроса.
                var proxy = WebRequest.GetSystemWebProxy();
                proxy.Credentials = CredentialCache.DefaultCredentials;
                var webRequest = (HttpWebRequest)WebRequest.Create(messageInfo.RerouteUrl);
                webRequest.Proxy = proxy;
                webRequest.KeepAlive = false;
                webRequest.Timeout = config.Timeout;
                webRequest.PreAuthenticate = true;
                webRequest.ContentType = messageInfo.ContentType;
                webRequest.Accept = messageInfo.AcceptTypes;
                webRequest.Method = messageInfo.HttpMethod;

                // Проверяем, может ли быть тело у этого запроса по его методу.
                if (RequestsWithMessageBody.Any(x => string.Equals(webRequest.Method, x, StringComparison.CurrentCultureIgnoreCase)))
                {
                    using (var stream = webRequest.GetRequestStream())
                    using (var writer = new StreamWriter(stream))
                        writer.Write(message);
                }

                // Выполнение запроса.
                string response;
                string clientAddress;
                try
                {
                    var webResponse = webRequest.GetResponse();
                    using (var rd = new StreamReader(webResponse.GetResponseStream()))
                        response = rd.ReadToEnd();
                    messageInfo.ContentType = webResponse.ContentType;
                    clientAddress = webResponse.ResponseUri.Host;
                }
                catch (WebException wex)
                {
                    using (var rd = new StreamReader(wex.Response.GetResponseStream()))
                        response = rd.ReadToEnd();
                    clientAddress = wex.Response.ResponseUri.Host;
                }

                // Имитация отправки сообщения через шину.
                ImitateServiceBusMessage(config.SenderId, config.ReceiverId, messageInfo.SbResponseType);

                // Возвращение ответа.
                ReturnAnswer(response, messageInfo.ContentType, messageInfo.ContextId);
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex, null, "Произошла ошибка при выполнении запроса.");
            }
        }

        /// <summary>
        /// Возвращает ответ отправителю изначального сообщения, если связь ещё не прервалась.
        /// </summary>
        /// <param name="response">Ответ на изначальное сообщение.</param>
        /// <param name="contentType">Тип содержимого.</param>
        /// <param name="contextId">ID сохранённого контекста.</param>
        private void ReturnAnswer(string response, string contentType, Guid contextId)
        {
            try
            {
                HttpListenerContext context;
                contexts.TryRemove(contextId, out context);

                context.Response.ContentType = contentType;
                ReturnText(context, response);
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex, null, "При отправке ответа на http-запрос произошла ошибка.");
            }
        }

        /// <summary>
        /// Возвращает клиенту указанный http-код.
        /// </summary>
        /// <param name="context">Контекст http-соединения.</param>
        /// <param name="code">Код http.</param>
        private static void ReturnCode(HttpListenerContext context, int code)
        {
            context.Response.StatusCode = code;
            var html = string.Format("<html><body><h1>{0} {1}</h1></body></html>", code, ((HttpStatusCode)code));
            ReturnText(context, html);
        }

        /// <summary>
        /// Возвращает клиенту указанный текст.
        /// </summary>
        /// <param name="context">Контекст http-соединения.</param>
        /// <param name="text">Текст для клиента.</param>
        private static void ReturnText(HttpListenerContext context, string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            context.Response.ContentType = context.Request.ContentType;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Возвращает клиенту содержимое wsdl-файла.
        /// </summary>
        /// <param name="context">Контекст http-соединения.</param>
        /// <param name="wsdlFilePath">Путь к файлу wsdl.</param>
        private static void ReturnWsdl(HttpListenerContext context, string wsdlFilePath)
        {
            if (File.Exists(wsdlFilePath))
            {
                var wsdl = File.ReadAllText(wsdlFilePath);
                ReturnText(context, wsdl);
            }
            else
                ReturnCode(context, 404);
        }

        /// <summary>
        /// Добавление тэга To в Header, если там его нету.
        /// Замена значения этого тэга на указанную ссылку.
        /// </summary>
        /// <param name="xmlDoc">Документ.</param>
        /// <param name="url">Ссылка.</param>
        private static void FixWsaTo(ref XmlDocument xmlDoc, string url)
        {
            if (xmlDoc.FirstChild == null)
                return;

            const string wsa = "http://www.w3.org/2005/08/addressing";
            var ns = new XmlNamespaceManager(xmlDoc.NameTable);
            ns.AddNamespace("wsa", wsa);

            var mainUri = xmlDoc.FirstChild.NamespaceURI;
            var headerNode = xmlDoc.FirstChild.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.LocalName == "Header" && x.NamespaceURI == mainUri);
            if (headerNode == null)
                return;

            var wsaTo = headerNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.LocalName == "To" && x.NamespaceURI == wsa);
            if (wsaTo == null)
            {
                wsaTo = xmlDoc.CreateElement("To", wsa);
                headerNode.AppendChild(wsaTo);
            }

            wsaTo.InnerText = url;
        }

        /// <summary>
        /// Imitates sending message throw SB.
        /// </summary>
        /// <param name="senderId">Sender of message.</param>
        /// <param name="receiverId">Receiver of message.</param>
        /// <param name="messageType">Type of message.</param>
        private void ImitateServiceBusMessage(string senderId, string receiverId, string messageType)
        {
            var subscriptions = _subscriptionsManager.GetSubscriptionsForMsgType(messageType, senderId);
            if (subscriptions == null || !subscriptions.Any())
                throw new Exception(string.Format("Не найдена подписка для типа сообщения {0}.", messageType));
            var subscription = subscriptions.First();

            var dummyMessageForEsb = new MessageForESB()
            {
                ClientID = senderId,
                MessageTypeID = messageType,
            };
            _logger.LogIncomingMessage(dummyMessageForEsb);
            _statisticsService.NotifyMessageSent(subscriptions.First());

            var dummyMessage = new Message()
            {
                Recipient = new Client() { ID = receiverId },
                MessageType = new MessageType() { ID = messageType },
            };
            _logger.LogOutgoingMessage(dummyMessage);
            _statisticsService.NotifyMessageReceived(subscriptions.First());
        }
    }
}
