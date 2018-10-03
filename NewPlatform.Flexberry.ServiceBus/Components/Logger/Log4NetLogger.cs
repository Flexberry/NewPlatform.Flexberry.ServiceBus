namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Логгер на основе Log4Net
    /// </summary>
    internal class Log4NetLogger : BaseServiceBusComponent, ILogger
    {
        private readonly LoggerHelper _loggerHelper;

        public Log4NetLogger(IDataService dataService)
        {
            _loggerHelper = new LoggerHelper(dataService);
        }

        public void LogUnhandledException(Exception exception, Message linkedMessage = null, string title = null, string message = null)
        {
            log4net.ThreadContext.Properties["title"] = title;
            log4net.ThreadContext.Properties["linkedMessage"] = linkedMessage?.__PrimaryKey;
            LogService.LogError(message, exception);
            if (linkedMessage != null)
            {
                LogService.LogDebug(
                    Environment.NewLine +
                    $"Получатель: \"{_loggerHelper.GetClientName(linkedMessage.Recipient.ID)}\"" + Environment.NewLine +
                    $"Тип сообщения: \"{_loggerHelper.GetMessageTypeName(linkedMessage.MessageType.ID)}\"" + Environment.NewLine +
                    $"Сообщение: {linkedMessage.Body}" + Environment.NewLine +
                    $"Вложение: {linkedMessage.Attachment}");
            }

            log4net.ThreadContext.Properties["title"] = string.Empty;
            log4net.ThreadContext.Properties["linkedMessage"] = string.Empty;
        }

        public void LogError(string title, string message, Message linkedMessage = null)
        {
            log4net.ThreadContext.Properties["title"] = title;
            log4net.ThreadContext.Properties["linkedMessage"] = linkedMessage?.__PrimaryKey;
            LogService.LogError(message);
            if (linkedMessage != null)
            {
                LogService.LogDebug(Environment.NewLine +
                    $"Получатель: \"{_loggerHelper.GetClientName(linkedMessage.Recipient.ID)}\"" + Environment.NewLine +
                    $"Тип сообщения: \"{_loggerHelper.GetMessageTypeName(linkedMessage.MessageType.ID)}\"" + Environment.NewLine +
                    $"Сообщение: {linkedMessage.Body}" + Environment.NewLine +
                    $"Вложение: {linkedMessage.Attachment}");
            }

            log4net.ThreadContext.Properties["title"] = string.Empty;
            log4net.ThreadContext.Properties["linkedMessage"] = string.Empty;
        }

        public void LogIncomingMessage(MessageForESB message)
        {
            log4net.ThreadContext.Properties["title"] = "Получено сообщение";
            LogService.LogInfo(
                $"Отправитель: \"{_loggerHelper.GetClientName(message.ClientID)}\". Тип сообщения \"{_loggerHelper.GetMessageTypeName(message.MessageTypeID)}\".");
            LogService.LogDebug(Environment.NewLine + $"Сообщение: {message.Body}" + Environment.NewLine + $"Вложение: {message.Attachment}");
            log4net.ThreadContext.Properties["title"] = string.Empty;
        }

        public void LogOutgoingMessage(Message message)
        {
            log4net.ThreadContext.Properties["title"] = "Передано сообщение";
            log4net.ThreadContext.Properties["linkedMessage"] = message?.__PrimaryKey;
            LogService.LogInfo(
                $"Получатель \"{_loggerHelper.GetClientName(message?.Recipient.ID)}\". Тип сообщения \"{_loggerHelper.GetMessageTypeName(message?.MessageType.ID)}\".");
            if (message != null)
            {
                LogService.LogDebug(Environment.NewLine + $"Сообщение: {message.Body}" + Environment.NewLine + $"Вложение: {message.Attachment}");
            }

            log4net.ThreadContext.Properties["linkedMessage"] = string.Empty;
            log4net.ThreadContext.Properties["title"] = string.Empty;
        }

        public void LogInformation(string title, string message)
        {
            log4net.ThreadContext.Properties["title"] = title;
            LogService.LogInfo(message);
            log4net.ThreadContext.Properties["title"] = string.Empty;
        }

        public void LogDebugMessage(string title, string message)
        {
            log4net.ThreadContext.Properties["title"] = title;
            LogService.LogDebug(message);
            log4net.ThreadContext.Properties["title"] = string.Empty;
        }
    }
}
