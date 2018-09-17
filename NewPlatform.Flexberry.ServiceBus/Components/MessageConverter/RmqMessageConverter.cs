using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Impl;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System.Collections.Generic;

    internal class MessageConverter
    {
        private string _attachmentPropertyName => "attachment";

        private string _bodyPropertyName => "body";

        private string _tagPropertiesPrefix = "__tag";

        /// <summary>
        /// Фомирование словаря "имя свойства - значения" для последующего построения тела JMS-сообщения.
        /// </summary>
        /// <param name="msg">Сообщение из шины.</param>
        /// <returns>Словарь "имя свойства - значения"</returns>
        public IDictionary<string, object> GetBodyProperties(MessageForESB msg)
        {
            return new Dictionary<string, object>()
            {
                {this._attachmentPropertyName, msg.Attachment },
                {this._bodyPropertyName, msg.Body }
            };
        }

        /// <summary>
        /// Получение словаря "имя свойства - значения" для добавления их в свойства JMS-сообщения.
        /// Используется для хранения тэгов.
        /// </summary>
        /// <param name="msg">Сообщение для шины.</param>
        /// <returns>Словарь "имя свойства - значения"</returns>
        public IDictionary<string, object> GetProperties(MessageForESB msg)
        {
            var properties = new Dictionary<string, object>();
            foreach (var tag in msg.Tags)
            {
                properties.Add(_tagPropertiesPrefix + tag.Key, tag.Value);
            }

            return properties;
        }

        /// <summary>
        /// Получение из свойств тела сообщения и свойств сообщения сообщения в формате шины.
        /// </summary>
        /// <param name="bodyProperties">Свойства тела сообщения.</param>
        /// <param name="properties">Свойства сообщения (тэги).</param>
        /// <returns>Сообщение в формате шины.</returns>
        public Message ConvertFromMqFormat(byte[] messagePayload, IDictionary<string, object> properties)
        {
            var result = new Message();

            BasicProperties rmqProperties = new RabbitMQ.Client.Framing.BasicProperties();
            rmqProperties.Headers = properties;
            var mapMessageReader = new MapMessageReader(rmqProperties, messagePayload);

            var attachment = (byte[])mapMessageReader.Body[this._attachmentPropertyName];
            var body = (string)mapMessageReader.Body[this._bodyPropertyName];

            var messageTags = new Dictionary<string, string>();
            foreach (var property in mapMessageReader.Properties.Headers)
            {
                if (property.Key.StartsWith(this._tagPropertiesPrefix))
                {
                    messageTags.Add(property.Key.Substring(this._tagPropertiesPrefix.Length), property.Value.ToString());
                }
            }

            result.BinaryAttachment = attachment;
            result.Body = body;
            result.Tags = messageTags.Select(x => $"{x.Key}:{x.Value}").Aggregate((x, y) => $"{x}, {y}");

            return result;
        }
    }
}
