namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components.SendingManager.RequestTemplates
{
    using System;

    public partial class HTTPRequestTemplate
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
    }
}
