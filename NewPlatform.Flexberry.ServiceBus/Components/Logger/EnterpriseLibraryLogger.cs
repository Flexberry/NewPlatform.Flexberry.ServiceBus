namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using ICSSoft.STORMNET.Business;
    using IIS.Caseberry.Logging.MsEntLib;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// Realization of logger using MS Enterprise Library Loggin through <see cref="CaseberryDatabaseTraceListener"/>.
    /// </summary>
    internal class EnterpriseLibraryLogger : BaseServiceBusComponent, ILogger
    {
        private readonly IDataService _dataService;

        private readonly LoggerHelper _loggerHelper;

        /// <summary>
        /// Possible log events.
        /// </summary>
        protected enum LogEventType
        {
            /// <summary>
            /// Exception occures.
            /// </summary>
            Exception = 1,

            /// <summary>
            /// Message was received by service bus.
            /// </summary>
            MessageReceived = 2,

            /// <summary>
            /// Message was sended by service bus.
            /// </summary>
            MessageSended = 3,

            /// <summary>
            /// Information message.
            /// </summary>
            InformationMessage = 5
        }

        /// <summary>
        /// Name of the source in Windows events log.
        /// </summary>
        private const string LogSourceName = "IIS Service Bus";

        /// <summary>
        /// Флаг, определяющий, нужно ли добавлять в лог информационные сообщения.
        /// </summary>
        public bool EnableInformationLogging { get; set; } = true;

        public EnterpriseLibraryLogger(IDataService dataService)
        {
            _dataService = dataService;
            _loggerHelper = new LoggerHelper(dataService);
            CaseberryDatabaseTraceListener.NewLogEntryAdded += NewLogEntryAdded;
        }

        /// <summary>
        /// Преобразовать исключение в строковый вид. Если <see cref="ExceptionFormatter"/> успешно обрабатывает переданное исключение, используется он.
        /// Иначе генерируется строка, состоящая из последовательности записей о вложенных исключениях, каждая запись содержит сообщение и стек исключения.
        /// </summary>
        /// <param name="ex">Исключение, которое нужно преобразовать в строку.</param>
        /// <returns>
        /// Строка с сообщением об исключении.
        /// </returns>
        protected static string GetDeepExceptionMessage(Exception ex)
        {
            try
            {
                return new ExceptionFormatter().GetMessage(ex);
            }
            catch (Exception)
            {
                string errorMessage = string.Empty;
                var currentErrorLevel = ex;
                while (currentErrorLevel != null)
                {
                    string errorMessageLocal = currentErrorLevel.Message;
                    string errorStackLocal = currentErrorLevel.StackTrace;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        errorMessage += string.Format("{0}InnerException:{0}", Environment.NewLine);
                    }

                    errorMessage += errorMessageLocal;

                    if (errorStackLocal != null)
                    {
                        errorMessage += string.Format("{0}StackTrace:", Environment.NewLine);
                    }

                    currentErrorLevel = currentErrorLevel.InnerException;
                }

                return errorMessage;
            }
        }

        /// <summary>
        /// Запись сообщения в события Windows.
        /// </summary>
        /// <param name="message">Сообщение для записи в лог.</param>
        /// <param name="eventLogEntryType">Статус сообщения: ошибка, информация или др.</param>
        protected static void WriteToEventLog(string message, EventLogEntryType eventLogEntryType = EventLogEntryType.Information)
        {
            EventLog.WriteEntry(LogSourceName, message, eventLogEntryType);
        }

        /// <summary>
        /// Добавить запись в лог Enterprise Library.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно добавить.</param>
        /// <param name="category">Категория сообщения (произвольная строка).</param>
        /// <param name="priority">Приоритет добавляемого сообщения (0 - самый высокий).</param>
        /// <param name="eventId">Идентификатор события. Одно из значений <see cref="LogEventType"/>.</param>
        /// <param name="severity">Критичность сообщения.</param>
        /// <param name="title">Заголовок сообщения.</param>
        /// <param name="lastMsg">Последнее сообщение, с которым выполнялись какие-то действия.</param>
        protected virtual void WriteToLog(
            object message,
            string category,
            int priority,
            LogEventType eventId,
            TraceEventType severity,
            string title = null,
            Message lastMsg = null)
        {
            if (severity == TraceEventType.Information && !EnableInformationLogging)
                return;

            try
            {
                LinkedMsg = lastMsg;

                var logEntry = new LogEntry
                {
                    Message = message.ToString(),
                    Priority = priority,
                    EventId = (int)eventId,
                    Severity = severity,
                    Title = title,
                    Categories = new Collection<string> { category },
                    TimeStamp = DateTime.Now,
                    ActivityId = Guid.NewGuid(),
                };

                Logger.Write(logEntry);
            }
            catch (Exception exception)
            {
                WriteToEventLog(string.Format("Не удалось записать данные в лог: {0}", GetDeepExceptionMessage(exception)), EventLogEntryType.Error);
                WriteToEventLog(message.ToString());
            }
        }

        /// <summary>
        /// Обработчик добавления записи лога для <see cref="CaseberryDatabaseTraceListener"/>.
        /// Добавляет связь между сообщением шины и записью лога при условии, что запись относится к этому сообщению.
        /// </summary>
        /// <param name="sender">Источник события добавления записи лога.</param>
        /// <param name="eventArgs">Аргументы события.</param>
        protected virtual void NewLogEntryAdded(object sender, DataObjectIdEventArgs eventArgs)
        {
            try
            {
                var msg = LinkedMsg;

                if (msg == null) return;

                if (string.IsNullOrEmpty(msg.Logs)) msg.Logs = eventArgs.DataObjectId.ToString();
                else msg.Logs += ';' + eventArgs.DataObjectId.ToString();
                _dataService.UpdateObject(msg);
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex);
            }
            finally
            {
                LinkedMsg = null;
            }
        }

        /// <summary>
        /// Поле для хранения ссылки на сообщение, связанное с записываемой в лог ошибкой.
        /// Используется, так как нет другой возможности передать сообщение в обработчик события добавления записи лога (<see cref="NewLogEntryAdded"/>).
        /// </summary>
        protected Message LinkedMsg;

        /// <summary>
        /// Действия, которые необходимо выполнить после остановки компонента. Полное завершение работы сервиса.
        /// </summary>
        public override void AfterStop()
        {
            CaseberryDatabaseTraceListener.NewLogEntryAdded -= NewLogEntryAdded;
        }

        /// <summary>
        /// Добавить в лог запись об необработанном исключении.
        /// </summary>
        /// <param name="exception">Объект исключения, которое было брошено.</param>
        /// <param name="linkedMessage">Сообщение (если оно есть), при работе с которым возникло исключение. Если сообщения нет, передается <c>null</c>.</param>
        /// <param name="title">Заголовок записи лога. Если не задан, берется из исключения.</param>
        /// <param name="message">Сообщение лога. Задается в случае, если нужно переопределить сообщение из исключения.</param>
        public virtual void LogUnhandledException(Exception exception, Message linkedMessage = null, string title = null, string message = null)
        {
            WriteToLog(
                string.Format("{0} {1}", message, GetDeepExceptionMessage(exception)),
                "Runtime exception",
                0,
                LogEventType.Exception,
                TraceEventType.Error,
                title ?? "Runtime exception",
                linkedMessage);
        }

        /// <summary>
        /// Добавить в лог запись о произошедшей ошибке.
        /// </summary>
        /// <param name="title">Заголовок сообщения об ошибке.</param>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="linkedMessage">Сообщение (если оно есть), при работе с которым произошла ошибка. Если сообщения нет, передается <c>null</c>.</param>
        public virtual void LogError(string title, string message, Message linkedMessage = null)
        {
            WriteToLog(
                message,
                "Error",
                0,
                LogEventType.Exception,
                TraceEventType.Error,
                title ?? "Error",
                linkedMessage);
        }

        /// <summary>
        /// Добавить в лог запись о входящем сообщении в шину.
        /// </summary>
        /// <param name="message">Структура данных, описывающая пришедшее сообщение.</param>
        public virtual void LogIncomingMessage(ServiceBusMessage message)
        {
            WriteToLog(
                string.Format("Получено сообщение от {0}. Тип сообщения {1}.", _loggerHelper.GetClientName(message.ClientID), _loggerHelper.GetMessageTypeName(message.MessageTypeID)),
                "MessagesMoving",
                2,
                LogEventType.MessageReceived,
                TraceEventType.Information,
                "Получено сообщение");
        }

        /// <summary>
        /// Добавить в лог запись об исходящем сообщении из шины.
        /// </summary>
        /// <param name="message">Объект сообщения, которое было отправлено.</param>
        public virtual void LogOutgoingMessage(Message message)
        {
            WriteToLog(
                string.Format("Передано сообщение {0}. Тип сообщения {1}.", _loggerHelper.GetClientName(message.Recipient.ID), _loggerHelper.GetMessageTypeName(message.MessageType.ID)),
                "MessagesMoving",
                2,
                LogEventType.MessageSended,
                TraceEventType.Information,
                "Передано сообщение");
        }

        /// <summary>
        /// Добавить в лог информационное сообщение.
        /// </summary>
        /// <param name="title">Заголовок сообщения.</param>
        /// <param name="message">Текст сообщения.</param>
        public virtual void LogInformation(string title, string message)
        {
            if (string.IsNullOrEmpty(title))
                title = "Информация";

            WriteToLog(
                message,
                "Information",
                2,
                LogEventType.InformationMessage,
                TraceEventType.Information,
                title);
        }

        /// <summary>
        /// Добавить в лог отладочное сообщение.
        /// </summary>
        /// <param name="title">Заголовок сообщения.</param>
        /// <param name="message">Текст сообщения.</param>
        public virtual void LogDebugMessage(string title, string message)
        {
            WriteToLog(
                message,
                "Debug",
                2,
                LogEventType.InformationMessage,
                TraceEventType.Information,
                title);
        }
    }
}
