using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus
{
    /// <summary>
    /// Компонент преобразования сообщения шины в формат JMS (заголовок и тело)
    /// </summary>
    public interface IMessageConverter
    {
        IDictionary<string, object> GetBodyProperties(MessageForESB msg);

        IDictionary<string, object> GetProperties(MessageForESB msg);

        Message ConvertFromMqFormat(byte[] messagePayload, IDictionary<string, object> properties);
    }
}
