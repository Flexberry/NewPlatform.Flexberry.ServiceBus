﻿namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components.SendingManager.RequestTemplates
{
    using System;

    public class WCFRequestTemplate
    {
        public Message Model { get; set; }

        public override string ToString()
        {
            string[] tags = Model.Tags.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string tagsArray = string.Empty;
            foreach (var tag in tags)
            {
                tagsArray += @"<c:KeyValueOfstringstring>\s*<c:Key>" + tag.Split(':')[0] + @"</c:Key>\s*<c:Value>" + tag.Split(':')[1] + @"</c:Value>\s*</c:KeyValueOfstringstring>\s*";
            }

            return
@"\s*
<s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"" xmlns:a=""http://www.w3.org/2005/08/addressing"">\s*
<s:Header>\s*
<a:Action s:mustUnderstand=""1"">http://tempuri.org/ICallbackSubscriber/AcceptMessage</a:Action>\s*
<a:MessageID>urn:uuid:[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}</a:MessageID>\s*
<a:ReplyTo>\s*
<a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>\s*
</a:ReplyTo>\s*
<a:To s:mustUnderstand=""1"">" + Model.Recipient.Address + @"</a:To>\s*
</s:Header>\s*
<s:Body>\s*
<AcceptMessage xmlns=""http://tempuri.org/"">\s*
<msg xmlns:b=""http://schemas.datacontract.org/2004/07/IIS.Persona.ServiceBus.Objects"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">\s*"
+ ((Model.Attachment == null) ? @"
<b:Attachment/>\s*" : @"
<b:Attachment>" + Model.Attachment + @"</b:Attachment>\s*") + @"
<b:Body>" + Model.Body + @"</b:Body>\s*"
+ ((Model.Group == null) ? @"
<b:GroupID i:nil=""true""/>\s*"
: @"
<b:GroupID>" + Model.Group + @"</b:GroupID>\s*") + @"
<b:MessageFormingTime>" + Model.ReceivingTime.ToString("yyyy-MM-ddTHH:mm:ss.ff") + @"[\d]?</b:MessageFormingTime>\s*
<b:MessageTypeID>" + Model.MessageType.ID + @"</b:MessageTypeID>\s*"
+ ((Model.Sender == null) ? @"
<b:SenderName i:nil=""true""/>\s*"
: @"
<b:SenderName>" + Model.Sender + @"</b:SenderName>\s*")
+ ((Model.Tags == null) ? @"
<b:Tags xmlns:c=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/>\s*"
: @"
<b:Tags xmlns:c=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">\s*
" + tagsArray + @"
</b:Tags>\s*"
) + @"
</msg>\s*
</AcceptMessage>\s*
</s:Body>\s*
</s:Envelope>\s*";
        }
    }
}
