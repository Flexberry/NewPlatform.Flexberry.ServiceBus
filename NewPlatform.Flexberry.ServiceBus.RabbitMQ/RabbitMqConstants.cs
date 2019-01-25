using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus
{
    public class RabbitMqConstants
    {
        public class FlexberryArgumentsKeys
        {
            public const string NotSyncFlag = "x-flexberry-not-sync";
        }

        public class FlexberryHeadersKeys
        {
            public const string OriginalMessageTimestamp = "x-flexberry-original-timestamp";
        }
    }
}
