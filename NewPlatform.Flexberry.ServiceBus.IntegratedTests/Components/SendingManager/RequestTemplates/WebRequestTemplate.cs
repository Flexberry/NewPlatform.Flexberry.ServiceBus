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
                tagsArray += @"<b:KeyValueOfstringstring>\s*<b:Key>" + tag.Split(':')[0] + @"</b:Key>\s*<b:Value>" + tag.Split(':')[1] + @"</b:Value>\s*</b:KeyValueOfstringstring>\s*";
            }

            return
@"\s*
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">\s*
<s:Header>\s*
<headerName xmlns=""http://localhost:2525/Message"">headerValue</headerName>\s*
</s:Header>\s*
<s:Body>\s*
<AcceptMessage xmlns=""http://tempuri.org/"">\s*
<msg xmlns:a=""http://schemas.datacontract.org/2004/07/IIS.Persona.ServiceBus.Objects"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">\s*"
+ ((Model.Attachment == null) ? @"
<a:Attachment/>\s*" : @"
<a:Attachment>" + Model.Attachment + @"</a:Attachment>\s*") + @"
<a:Body>" + Model.Body + @"</a:Body>\s*"
+ ((Model.Group == null) ? @"
<a:GroupID i:nil=""true""/>\s*"
: @"
<a:GroupID>" + Model.Group + @"</a:GroupID>\s*") + @"
<a:MessageFormingTime>" + Model.ReceivingTime.ToString("yyyy-MM-ddTHH:mm:ss.ff") + @"[\d]?</a:MessageFormingTime>\s*
<a:MessageTypeID>" + Model.MessageType.ID + @"</a:MessageTypeID>\s*"
+ ((Model.Sender == null) ? @"
<a:SenderName i:nil=""true""/>\s*"
: @"
<a:SenderName>" + Model.Sender + @"</a:SenderName>\s*")
+ ((Model.Tags == null) ? @"
<a:Tags xmlns:b=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/>\s*"
: @"
<a:Tags xmlns:b=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">\s*
" + tagsArray + @"
</a:Tags>\s*"
) + @"
</msg>\s*
</AcceptMessage>\s*
</s:Body>\s*
</s:Envelope>\s*";
        }
    }
}
