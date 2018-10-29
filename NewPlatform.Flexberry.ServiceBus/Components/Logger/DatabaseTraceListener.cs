namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using ICSSoft.STORMNET.Business;
    using IIS.Caseberry.Logging.Objects;

    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Database.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
    using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

    /// <summary>
    /// A <see cref="System.Diagnostics.TraceListener"/> that writes to a database, formatting the output with an <see cref="ILogFormatter"/>.
    /// </summary>
    [ConfigurationElementType(typeof(FormattedDatabaseTraceListenerData))]
    internal class DatabaseTraceListener : FormattedTraceListenerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTraceListener"/> class.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        public DatabaseTraceListener(ILogFormatter formatter)
            : base(formatter)
        {
        }

        /// <summary>
        /// Event when adding a new log entry.
        /// </summary>
        public static event EventHandler<DataObjectIdEventArgs> NewLogEntryAdded;

        /// <summary>
        /// Event handler.
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        public static void OnNewLogEntryAdded(DataObjectIdEventArgs e)
        {
            NewLogEntryAdded?.Invoke(null, e);
        }

        /// <summary>
        /// The Write method.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public override void Write(string message)
        {
            this.WriteLog(
                0,
                5,
                TraceEventType.Information,
                string.Empty,
                DateTime.Now,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                null,
                message);
        }

        /// <summary>
        /// The WriteLine method.
        /// </summary>
        /// <param name="message">The message to log</param>
        public override void WriteLine(string message)
        {
            this.Write(message);
        }

        /// <summary>
        /// Delivers the trace data to the underlying database.
        /// </summary>
        /// <param name="eventCache">The context information provided by <see cref="System.Diagnostics"/>.</param>
        /// <param name="source">The name of the trace source that delivered the trace data.</param>
        /// <param name="eventType">The type of event.</param>
        /// <param name="id">The id of the event.</param>
        /// <param name="data">The data to trace.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                if (data is LogEntry)
                {
                    var logEntry = data as LogEntry;

                    this.WriteLog(logEntry);
                }
                else if (data is string)
                {
                    this.Write(data as string);
                }
                else
                {
                    base.TraceData(eventCache, source, eventType, id, data);
                }
            }
        }

        /// <summary>
        /// Declare the supported attributes for <see cref="DatabaseTraceListener"/>.
        /// </summary>
        /// <returns>Supported attributes.</returns>
        protected override string[] GetSupportedAttributes()
        {
            return new[] { "formatter" };
        }

        /// <summary>
        /// Executes the WriteLog stored procedure.
        /// </summary>
        /// <param name="eventId">The event id for this LogEntry.</param>
        /// <param name="priority">The priority for this LogEntry.</param>
        /// <param name="severity">The severity for this LogEntry.</param>
        /// <param name="title">The title for this LogEntry.</param>
        /// <param name="timeStamp">The timestamp for this LogEntry.</param>
        /// <param name="machineName">The machine name for this LogEntry.</param>
        /// <param name="appDomainName">The appDomainName for this LogEntry.</param>
        /// <param name="processId">The process id for this LogEntry.</param>
        /// <param name="processName">The processName for this LogEntry.</param>
        /// <param name="managedThreadName">The managedthreadName for this LogEntry.</param>
        /// <param name="win32ThreadId">The win32threadID for this LogEntry.</param>
        /// <param name="message">The message for this LogEntry.</param>
        private void WriteLog(
            int eventId,
            int priority,
            TraceEventType severity,
            string title,
            DateTime timeStamp,
            string machineName,
            string appDomainName,
            string processId,
            string processName,
            string managedThreadName,
            string win32ThreadId,
            string message)
        {
            var appLogEntry = new ApplicationLog()
            {
                EventId = eventId,
                Priority = priority,
                Severity = severity.ToString(),
                Title = title,
                Timestamp = timeStamp,
                MachineName = machineName,
                AppDomainName = appDomainName,
                ProcessId = processId,
                ProcessName = processName,
                ThreadName = managedThreadName,
                Win32ThreadId = win32ThreadId,
                Message = message,
                FormattedMessage = message,
            };

            DataServiceProvider.DataService.UpdateObject(appLogEntry);
        }

        /// <summary>
        /// Executes the WriteLog stored procedure.
        /// </summary>
        /// <param name="logEntry">The LogEntry to store in the database.</param>
        private void WriteLog(LogEntry logEntry)
        {
            var appLogEntry = new ApplicationLog ()
            {
                Category = logEntry.Categories.FirstOrDefault(),
                EventId = logEntry.EventId,
                Priority = logEntry.Priority,
                Severity = logEntry.Severity.ToString(),
                Title = logEntry.Title,
                Timestamp = logEntry.TimeStamp,
                MachineName = logEntry.MachineName,
                AppDomainName = logEntry.AppDomainName,
                ProcessId = logEntry.ProcessId,
                ProcessName = logEntry.ProcessName,
                ThreadName = logEntry.ManagedThreadName,
                Win32ThreadId = logEntry.Win32ThreadId,
                Message = logEntry.Message,
                FormattedMessage = this.Formatter != null ? this.Formatter.Format(logEntry) : logEntry.Message,
            };

            DataServiceProvider.DataService.UpdateObject(appLogEntry);
        }
    }

}
