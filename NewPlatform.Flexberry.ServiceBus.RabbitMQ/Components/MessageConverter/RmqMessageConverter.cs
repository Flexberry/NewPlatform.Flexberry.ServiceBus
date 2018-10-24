using System;
using System.Linq;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Impl;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System.Collections.Generic;

    internal class MessageConverter : IMessageConverter
    {
        private string _attachmentPropertyName => "attachment";

        private string _bodyPropertyName => "body";

        private string _senderIdPropepertyName => "senderId";

        /// <summary>
        /// Свойство сообщения RabbitMQ, в котором хранится Timestamp сообщения
        /// </summary>
        protected string TimestampPropertyName => "timestamp_in_ms";

        private string _tagPropertiesPrefix = "__tag";

        /// <summary>
        /// Фомирование словаря "имя свойства - значения" для последующего построения тела JMS-сообщения.
        /// </summary>
        /// <param name="msg">Сообщение из шины.</param>
        /// <returns>Словарь "имя свойства - значения"</returns>
        public IDictionary<string, object> GetBodyProperties(ServiceBusMessage msg)
        {
            var result = new Dictionary<string, object>();

            if (msg.Attachment != null && msg.Attachment.Any())
            {
                result[this._attachmentPropertyName] = msg.Attachment;
            }

            if (msg.Body != null)
            {
                result[this._bodyPropertyName] = msg.Body;
            }

            if (msg.ClientID != null)
            {
                result[this._senderIdPropepertyName] = msg.ClientID;
            }

            return result;
        }

        /// <summary>
        /// Получение словаря "имя свойства - значения" для добавления их в свойства JMS-сообщения.
        /// Используется для хранения тэгов.
        /// </summary>
        /// <param name="msg">Сообщение для шины.</param>
        /// <returns>Словарь "имя свойства - значения"</returns>
        public IDictionary<string, object> GetProperties(ServiceBusMessage msg)
        {
            var properties = new Dictionary<string, object>();
            if (msg.Tags != null)
            {
                foreach (var tag in msg.Tags)
                {
                    properties.Add(_tagPropertiesPrefix + tag.Key, tag.Value);
                }
            }

            return properties;
        }

        /// <summary>
        /// Получение из свойств тела сообщения и свойств сообщения сообщения в формате шины.
        /// </summary>
        /// <param name="bodyProperties">Свойства тела сообщения.</param>
        /// <param name="properties">Свойства сообщения (тэги).</param>
        /// <returns>Сообщение в формате шины.</returns>
        public MessageWithNotTypedPk ConvertFromMqFormat(byte[] messagePayload, IDictionary<string, object> properties)
        {
            var result = new MessageWithNotTypedPk();

            BasicProperties rmqProperties = new RabbitMQ.Client.Framing.BasicProperties();
            rmqProperties.Headers = properties;
            var mapMessageReader = new MapMessageReader(rmqProperties, messagePayload);

            var bodyProperties = mapMessageReader.Body;

            if (bodyProperties.ContainsKey(this._bodyPropertyName))
            {
                result.Body = (string) mapMessageReader.Body[this._bodyPropertyName];
            }

            if (bodyProperties.ContainsKey(this._attachmentPropertyName))
            {
                result.BinaryAttachment = (byte[])mapMessageReader.Body[this._attachmentPropertyName];
            }

            if (bodyProperties.ContainsKey(this._senderIdPropepertyName))
            {
                result.Sender = (string)mapMessageReader.Body[this._senderIdPropepertyName];
            }

            var headers = mapMessageReader.Properties.Headers;
            if (headers != null)
            {
                var messageTags = new Dictionary<string, string>();
                foreach (var property in headers)
                {
                    if (property.Key.StartsWith(this._tagPropertiesPrefix))
                    {
                        messageTags.Add(property.Key.Substring(this._tagPropertiesPrefix.Length), property.Value.ToString());
                    }
                }

                if (headers.ContainsKey(this.TimestampPropertyName))
                {
                    var unixtimestamp = (long)headers[this.TimestampPropertyName];
                    result.ReceivingTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(unixtimestamp);
                }

                result.Tags = messageTags.Any() ? messageTags.Select(x => $"{x.Key}:{x.Value}").Aggregate((x, y) => $"{x}, {y}") : "";
            }

            return result;
        }

        /// <summary>
        /// Получить префикс для тэгов.
        /// </summary>
        /// <param name="tag">Имя тэга.</tag>
        /// <returns>Префикс для тэгов.</returns>
        public string GetTagPropertiesPrefix(string tag)
        {
            return $"{_tagPropertiesPrefix}{tag}";
        }
    }
}
