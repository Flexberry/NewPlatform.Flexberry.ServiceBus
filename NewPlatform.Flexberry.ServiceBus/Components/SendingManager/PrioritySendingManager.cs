namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.FunctionalLanguage.SQLWhere;
    using ICSSoft.STORMNET.KeyGen;

    using NewPlatform.Flexberry.ServiceBus.MessageSenders;

    /// <summary>
    /// Implementation of <see cref="ISendingManager"/> component with support <see cref="Client.ConnectionsLimit"/> and <see cref="Client.SequentialSent"/> options.
    /// </summary>
    internal class PrioritySendingManager : BaseSendingManager
    {
        /// <summary>
        /// Gets or sets a value indicating whether enable online state.
        /// </summary>
        public bool EnableOnlineState { get; set; } = false;

        /// <summary>
        /// Gets or sets DefaultConnectionsLimit.
        /// Used if the client does not have a connection limit.
        /// </summary>
        public int DefaultConnectionsLimit { get; set; } = 10;

        /// <summary>
        /// The maximum number of tasks for sending messages.
        /// </summary>
        public int MaxTasks { get; set; } = 1000;

        /// <summary>
        /// Number of minutes to be added to delay before the next attempt to send message.
        /// </summary>
        public int AdditionalMinutesBetweenRetries { get; set; } = 3;

        /// <summary>
        /// Current sending tasks for each client.
        /// </summary>
        private Dictionary<Guid, int> _clientConnections = new Dictionary<Guid, int>();

        /// <summary>
        /// Current sending tasks count.
        /// </summary>
        private int _sendingTasksCount;

        /// <summary>
        /// Object for locking.
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Timer for running <see cref="ScanMessages"/> method.
        /// </summary>
        private Timer _scanningTimer;

        /// <summary>
        /// Structure for <see cref="SendMessage"/> method.
        /// </summary>
        private struct SendingTaskParam
        {
            /// <summary>
            /// Subscription for sending a message.
            /// </summary>
            public Subscription Subscription;

            /// <summary>
            /// The message you are sending.
            /// </summary>
            public Message Message;
        }

        /// <summary>
        /// ISubscriptionsManager component.
        /// </summary>
        private readonly ISubscriptionsManager _subscriptionsManager;

        /// <summary>
        /// IStatisticsService component.
        /// </summary>
        private readonly IStatisticsService _statisticsService;

        /// <summary>
        /// Current DataService.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// ILogger component.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrioritySendingManager"/> class.
        /// </summary>
        /// <param name="subscriptionsManager">ISubscriptionsManager component.</param>
        /// <param name="statisticsService">IStatisticsService component.</param>
        /// <param name="dataService">Current DataService.</param>
        /// <param name="logger">ILogger component.</param>
        /// <param name="useLegacySenders">If <c>true</c>, previous versions of the interfaces will be used to send messages.</param>
        public PrioritySendingManager(ISubscriptionsManager subscriptionsManager, IStatisticsService statisticsService, IDataService dataService, ILogger logger, bool useLegacySenders = true)
            : base(subscriptionsManager, statisticsService, dataService, logger, useLegacySenders)
        {
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Try to send a message now.
        /// </summary>
        /// <param name="message">Message to sent.</param>
        public override void QueueForSending(Message message)
        {
            TryEnqueue(message);
        }

        /// <summary>
        /// Starts a periodic database scan for messages to send.
        /// </summary>
        public override void Start()
        {
            base.Start();
            _scanningTimer = new Timer(ScanMessages, null, 0, ScanningPeriodMilliseconds);
        }

        /// <summary>
        /// Stops a periodic database scan.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            _scanningTimer.Dispose();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanningTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Selects messages from the database that can be sent.
        /// </summary>
        /// <param name="state">Not used.</param>
        private void ScanMessages(object state)
        {
            try
            {
                if (_sendingTasksCount >= MaxTasks)
                {
                    return;
                }

                IEnumerable<Subscription> subscriptions = _subscriptionsManager.GetCallbackSubscriptions();
                if (!subscriptions.Any())
                {
                    return;
                }

                foreach (var clientSubscriptions in subscriptions.GroupBy(s => s.Client))
                {
                    int currentConnections = 0;
                    int connectionsLimit = clientSubscriptions.Key.ConnectionsLimit ?? DefaultConnectionsLimit;
                    Guid clientId = (KeyGuid)clientSubscriptions.Key.__PrimaryKey;
                    if (!_clientConnections.TryGetValue(clientId, out currentConnections))
                    {
                        _clientConnections.Add(clientId, 0);
                    }

                    if (currentConnections >= connectionsLimit)
                    {
                        continue;
                    }

                    SQLWhereLanguageDef langDef = SQLWhereLanguageDef.LanguageDef;
                    LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Message), Message.Views.SendingByCallbackView);

                    // All messages for this client by all its active subscriptions
                    Function clientLimitFunction = langDef.GetFunction(langDef.funcOR, clientSubscriptions.Select(s => langDef.GetFunction(
                        langDef.funcAND,
                        langDef.GetFunction(langDef.funcEQ, new VariableDef(langDef.GuidType, Information.ExtractPropertyPath<Message>(m => m.Recipient)), s.Client.__PrimaryKey),
                        langDef.GetFunction(langDef.funcEQ, new VariableDef(langDef.GuidType, Information.ExtractPropertyPath<Message>(m => m.MessageType)), s.MessageType.__PrimaryKey))).ToArray());

                    // Only unsent messages whose sending time has already arrived
                    lcs.LimitFunction = langDef.GetFunction(
                        langDef.funcAND,
                        clientLimitFunction,
                        langDef.GetFunction(langDef.funcEQ, new VariableDef(langDef.BoolType, Information.ExtractPropertyPath<Message>(m => m.IsSending)), false),
                        langDef.GetFunction(langDef.funcLEQ, new VariableDef(langDef.DateTimeType, Information.ExtractPropertyPath<Message>(m => m.SendingTime)), DateTime.Now));

                    // Get no more than we can send
                    lcs.ReturnTop = connectionsLimit - currentConnections;
                    lcs.ColumnsSort = new[]
                    {
                        new ColumnsSortDef(Information.ExtractPropertyPath<Message>(m => m.Priority), SortOrder.Asc),
                        new ColumnsSortDef(Information.ExtractPropertyPath<Message>(m => m.SendingTime), SortOrder.Asc),
                    };

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    DataObject[] messages = _dataService.LoadObjects(lcs);
                    stopwatch.Stop();
                    _statisticsService.NotifyAvgTimeSql(null, (int)stopwatch.ElapsedMilliseconds, $"PrioritySendingManager.ScanMessages(): Load {lcs.ReturnTop} messages for client with name: {clientSubscriptions.Key.Name}.");

                    int index = 0;
                    while (index < messages.Length && TryEnqueue((Message)messages[index]))
                        index++;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("An error occurred while scanning messages.", exception.ToString());
            }
        }

        /// <summary>
        /// Try to start sending a message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns><c>true</c> if sending was started, else <c>false</c>.</returns>
        private bool TryEnqueue(Message message)
        {
            Subscription subscription = _subscriptionsManager.GetSubscriptions(message.Recipient.ID).First(x => x.MessageType.ID == message.MessageType.ID);
            Guid clientId = (KeyGuid)subscription.Client.__PrimaryKey;
            int connectionsLimit = subscription.Client.ConnectionsLimit ?? DefaultConnectionsLimit;
            bool canSend = false;
            lock (_lock)
            {
                if (_clientConnections[clientId] < connectionsLimit && _sendingTasksCount < MaxTasks)
                {
                    _clientConnections[clientId]++;
                    _sendingTasksCount++;
                    canSend = true;
                }
            }

            if (canSend)
            {
                message.IsSending = true;
                Stopwatch stopwatch = Stopwatch.StartNew();
                _dataService.UpdateObject(message);
                stopwatch.Stop();
                _statisticsService.NotifyAvgTimeSql(subscription, (int)stopwatch.ElapsedMilliseconds, "PrioritySendingManager.TryEnqueue(): Update message sending status.");
                if (EnableOnlineState)
                {
                    _statisticsService.NotifyIncConnectionCount(subscription, message);
                }
                else
                {
                    _statisticsService.NotifyIncConnectionCount(subscription);
                }

                Task<bool>.Factory.StartNew(
                    SendMessage,
                    new SendingTaskParam() { Message = message, Subscription = subscription },
                    TaskCreationOptions.PreferFairness)
                    .ContinueWith(SendingTaskContinuation, message);
            }

            return canSend;
        }

        /// <summary>
        /// Sending message method.
        /// </summary>
        /// <param name="state"><see cref="SendingTaskParam"/> for sending.</param>
        /// <returns><c>true</c> if message was sent, else <c>false</c>.</returns>
        private bool SendMessage(object state)
        {
            var send = false;
            var param = (SendingTaskParam)state;
            IMessageSender messageSender = MessageSenderCreator.GetMessageSender(param.Subscription);
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                send = messageSender.SendMessage(param.Message);
                stopwatch.Stop();
                _statisticsService.NotifyAvgTimeSent(param.Subscription, (int)stopwatch.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                _logger.LogError("An error occurred while sending a message.", exception.ToString(), param.Message);
            }

            if (send)
            {
                _logger.LogOutgoingMessage(param.Message);
                _statisticsService.NotifyMessageSent(param.Subscription);
            }
            else
            {
                _statisticsService.NotifyErrorOccurred(param.Subscription);
            }

            return send;
        }

        /// <summary>
        /// Updates message state after attempt to sent.
        /// </summary>
        /// <param name="task">Task with <see cref="SendMessage"/> method.</param>
        /// <param name="state">The message we were trying to send.</param>
        private void SendingTaskContinuation(Task<bool> task, object state)
        {
            var message = (Message)state;
            Subscription subscription = _subscriptionsManager.GetSubscriptions(message.Recipient.ID).First(s => s.MessageType.ID == message.MessageType.ID);

            lock (_lock)
            {
                _sendingTasksCount--;
                _clientConnections[(KeyGuid)message.Recipient.__PrimaryKey]--;
            }

            if (EnableOnlineState)
            {
                _statisticsService.NotifyDecConnectionCount(subscription, message);
            }
            else
            {
                _statisticsService.NotifyDecConnectionCount(subscription);
            }

            var existNewMessageWithGroup = false;
            if (!string.IsNullOrEmpty(message.Group))
            {
                LoadingCustomizationStruct lcs = MessageBS.GetMessagesWithGroupLCS(message.Recipient, message.MessageType, message.Group);
                lcs.LimitFunction = SQLWhereLanguageDef.LanguageDef.GetFunction(
                    SQLWhereLanguageDef.LanguageDef.funcAND,
                    SQLWhereLanguageDef.LanguageDef.GetFunction(
                        SQLWhereLanguageDef.LanguageDef.funcNEQ,
                        new VariableDef(SQLWhereLanguageDef.LanguageDef.GuidType, SQLWhereLanguageDef.StormMainObjectKey),
                        ((KeyGuid)message.__PrimaryKey).Guid),
                    lcs.LimitFunction);

                existNewMessageWithGroup = _dataService.GetObjectsCount(lcs) > 0;
            }

            if (existNewMessageWithGroup || (task.Status == TaskStatus.RanToCompletion && task.Result))
            {
                message.SetStatus(ObjectStatus.Deleted);
            }
            else
            {
                message.ErrorCount++;
                message.IsSending = false;

                if (!message.Recipient.SequentialSent)
                {
                    int timeoutInMinutes = AdditionalMinutesBetweenRetries * message.ErrorCount;
                    message.SendingTime = DateTime.Now + new TimeSpan(0, timeoutInMinutes, 0);
                }
            }

            ServiceHelper.UpdateObject(_dataService, message, _logger, _statisticsService);
        }
    }
}
