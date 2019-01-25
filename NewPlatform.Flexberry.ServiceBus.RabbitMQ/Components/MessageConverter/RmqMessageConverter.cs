namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using Newtonsoft.Json.Linq;
    using RabbitMQ.Client.Content;
    using RabbitMQ.Client.Impl;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class RmqMessageConverter : IMessageConverter
    {
        private string _attachmentPropertyName => "attachment";

        private string _bodyPropertyName => "body";

        private string _senderIdPropepertyName => "senderId";

        /// <summary>
        /// Свойство сообщения RabbitMQ, в котором хранится Timestamp сообщения
        /// </summary>
        protected string TimestampPropertyName => "timestamp_in_ms";

        /// <summary>
        /// Header's key for message timestamp before redeliver.
        /// </summary>
        protected string OriginalTimestampPropertyName => RabbitMqConstants.FlexberryHeadersKeys.OriginalMessageTimestamp;

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

        public long GetErrorsCount(IDictionary<string, object> headerProperties)
        {
            long redeliveryCount = 0;
            string deadLetterHeaderKey = "x-death";

            if (headerProperties.ContainsKey(deadLetterHeaderKey))
            {
                var recsObj = headerProperties[deadLetterHeaderKey];
                IList deadLetteringRecs = null;

                if (recsObj is string json)
                {
                    JArray array = JArray.Parse(json);
                    deadLetteringRecs = array.Select(x => x.ToObject<Dictionary<string, object>>()).ToList();
                }
                else
                {
                    deadLetteringRecs = (IList)recsObj;
                }
                
                var lastRecord = (Dictionary<string, object>)deadLetteringRecs[deadLetteringRecs.Count - 1];
                redeliveryCount = (long)lastRecord["count"];
            }

            return redeliveryCount;
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
                        var tagKey = property.Key.Substring(this._tagPropertiesPrefix.Length);
                        var value = Encoding.UTF8.GetString((byte[])property.Value);
                        messageTags.Add(tagKey, value);
                    }
                }

                string timestampKey = headers.ContainsKey(this.OriginalTimestampPropertyName) ? OriginalTimestampPropertyName :
                                      headers.ContainsKey(this.TimestampPropertyName) ? TimestampPropertyName : null;

                if (timestampKey != null)
                {
                    var unixtimestamp = long.Parse(headers[this.TimestampPropertyName].ToString());
                    result.ReceivingTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(unixtimestamp);
                }

                result.Tags = messageTags.Any() ? messageTags.Select(x => $"{x.Key}:{x.Value}").Aggregate((x, y) => $"{x}, {y}") : "";
                result.ErrorCount = (int)GetErrorsCount(headers);
            }

            return result;
        }

        /// <summary>
        /// Получить префикс для тэгов.
        /// </summary>
        /// <param name="tag">Имя тэга.</param>
        /// <returns>Префикс для тэгов.</returns>
        public string GetTagPropertiesPrefix(string tag)
        {
            return $"{_tagPropertiesPrefix}{tag}";
        }
    }
}
