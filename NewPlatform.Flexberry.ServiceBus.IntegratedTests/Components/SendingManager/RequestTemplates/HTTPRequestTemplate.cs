namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components.SendingManager.RequestTemplates
{
    using System;

    internal class HTTPRequestTemplate
    {
        private string tagsAsString;

        private Message model;

        public Message Model
        {
            get
            {
                return model;
            }

            set
            {
                model = value;
                var tags = (Model.Tags ?? string.Empty).Replace(":", @""":""").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                tagsAsString = tags.Length > 0 ? @"{""" + string.Join(@""",""", tags) + @"""}" : "null";
            }
        }

        public override string ToString()
        {
            return @"\s*
{\s*
""Id"":""" + Model.__PrimaryKey.ToString() + @""",\s*
""Body"":""" + Model.Body + @""",\s*
""MessageFormingTime"":""" + Model.ReceivingTime.ToString("yyyy-MM-ddTHH:mm:ss.ff") + @"[\d]?"",\s*
""MessageTypeID"":""" + Model.MessageType.ID + @""",\s*
""SenderName"":""" + (Model.Sender ?? "null") + @""",\s*
""GroupID"":""" + (Model.Group ?? "null") + @""",\s*
""Tags"":" + tagsAsString + @",\s*
""Attachment"":""" + (Model.Attachment ?? "null") + @"""\s*
}\s*";
        }
    }
}
