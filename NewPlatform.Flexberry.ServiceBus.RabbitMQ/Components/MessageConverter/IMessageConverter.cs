namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System.Collections.Generic;

    /// <summary>
    /// Компонент преобразования сообщения шины в формат JMS (заголовок и тело)
    /// </summary>
    public interface IMessageConverter
    {
        IDictionary<string, object> GetBodyProperties(ServiceBusMessage msg);

        IDictionary<string, object> GetProperties(ServiceBusMessage msg);

        MessageWithNotTypedPk ConvertFromMqFormat(byte[] messagePayload, IDictionary<string, object> properties);

        string GetTagPropertiesPrefix(string tag);

        long GetErrorsCount(IDictionary<string, object> headerProperties);
    }
}
