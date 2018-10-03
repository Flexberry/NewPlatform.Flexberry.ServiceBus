namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.Configuration;
    using ClientTools;
    using Components;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// В данном классе шина выступает в роли клиента, принимая сообщения
    /// </summary>
    public partial class SBService : ICallbackSubscriber
    {
        /// <summary>
        /// Принять сообщение от шины.
        /// </summary>
        /// <param name="msg">Принимаемое сообщение.</param>
        public void AcceptMessage(MessageFromESB msg)
        {
            var msgFor = new MessageForESB
                {
                    Attachment = msg.Attachment,
                    Body = msg.Body,
                    MessageTypeID = msg.MessageTypeID,
                    ClientID = GetLastSenderID(msg),
                    Tags = msg.Tags
                };

            if (!msgFor.Tags.ContainsKey("senderName"))
            {
                msgFor.Tags.Add("senderName", msg.SenderName);
            }

            if (msg.GroupID == String.Empty)
            {
                SendMessageToESB(msgFor);
            }
            else
            {
                SendMessageToESBWithUseGroup(msgFor, msg.GroupID);
            }
        }

        /// <summary>
        /// Получить идентификатор шины, выступающей в качестве клиента.
        /// </summary>
        /// <returns></returns>
        public string GetSourceId()
        {
            return ConfigurationManager.AppSettings["ServiceBusClientKey"];
        }

        /// <summary>
        /// Уведомить о событии.
        /// </summary>
        /// <param name="ИдТипаСобытия">Идентификатор типа события.</param>
        public void RiseEvent(string ИдТипаСобытия)
        {
            var uc = new UnityContainer();
            var cbcs = uc.Resolve<CrossBusCommunicationService>();
            _receivingManager.RaiseEvent(cbcs.ServiceID4SB, ИдТипаСобытия);
        }

        /// <summary>
        /// Извлечь из тегов идентификатор последнего отправителя.
        /// </summary>
        /// <param name="msg">Сообщение, из которого извлекается последний отправитель.</param>
        /// <returns>Идентификатор последнего отправителя.</returns>
        private static string GetLastSenderID(MessageFromESB msg)
        {
            string[] strings = msg.Tags["sendingWay"].Split('/');

            return strings[strings.Length - 1];
        }
    }
}