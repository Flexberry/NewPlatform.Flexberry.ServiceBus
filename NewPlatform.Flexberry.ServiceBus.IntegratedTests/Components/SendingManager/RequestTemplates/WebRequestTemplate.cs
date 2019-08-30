namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components.SendingManager.RequestTemplates
{
    using System;

    public partial class WebRequestTemplate
    {
        public Message Model { get; set; }

        public override string ToString()
        {
            string[] tags = Model.Tags.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string tagsArray = string.Empty;
            foreach (var tag in tags)
            {
                tagsArray += @"<[0-9a-z]{1,}:KeyValueOfstringstring>\s*<[0-9a-z]{1,}:Key>" + tag.Split(':')[0] + @"</[0-9a-z]{1,}:Key>\s*<[0-9a-z]{1,}:Value>" + tag.Split(':')[1] + @"</[0-9a-z]{1,}:Value>\s*</[0-9a-z]{1,}:KeyValueOfstringstring>\s*";
            }

            return
@"\s*
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">\s*
<s:Header>\s*
<headerName xmlns=""http://localhost:2525/Message"">headerValue</headerName>\s*
</s:Header>\s*
<s:Body>\s*
<AcceptMessage xmlns=""http://tempuri.org/"">\s*
<msg xmlns:[0-9a-z]{1,}=""http://schemas.datacontract.org/2004/07/IIS.Persona.ServiceBus.Objects"" xmlns:[0-9a-z]{1,}=""http://www.w3.org/2001/XMLSchema-instance"">\s*"
+ ((Model.Attachment == null) ? @"
<[0-9a-z]{1,}:Attachment/>\s*" : @"
<[0-9a-z]{1,}:Attachment>" + Model.Attachment + @"</[0-9a-z]{1,}:Attachment>\s*") + @"
<[0-9a-z]{1,}:Body>" + Model.Body + @"</[0-9a-z]{1,}:Body>\s*"
+ ((Model.Group == null) ? @"
<[0-9a-z]{1,}:GroupID i:nil=""true""/>\s*"
: @"
<[0-9a-z]{1,}:GroupID>" + Model.Group + @"</[0-9a-z]{1,}:GroupID>\s*") + @"
<[0-9a-z]{1,}:MessageFormingTime>" + Model.ReceivingTime.ToString("yyyy-MM-ddTHH:mm:ss.ff") + @"[\d]?</[0-9a-z]{1,}:MessageFormingTime>\s*
<[0-9a-z]{1,}:MessageTypeID>" + Model.MessageType.ID + @"</[0-9a-z]{1,}:MessageTypeID>\s*"
+ ((Model.Sender == null) ? @"
<[0-9a-z]{1,}:SenderName i:nil=""true""/>\s*"
: @"
<[0-9a-z]{1,}:SenderName>" + Model.Sender + @"</[0-9a-z]{1,}:SenderName>\s*")
+ ((Model.Tags == null) ? @"
<[0-9a-z]{1,}:Tags xmlns:[0-9a-z]{1,}=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/>\s*"
: @"
<[0-9a-z]{1,}:Tags xmlns:[0-9a-z]{1,}=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">\s*
" + tagsArray + @"
</[0-9a-z]{1,}:Tags>\s*"
) + @"
</msg>\s*
</AcceptMessage>\s*
</s:Body>\s*
</s:Envelope>\s*";
        }
    }
}
