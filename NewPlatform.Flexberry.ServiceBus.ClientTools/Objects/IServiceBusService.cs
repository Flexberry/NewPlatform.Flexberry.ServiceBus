﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.Xml;
    
    
    // *** Start programmer edit section *** (Using statements)
    using System.ServiceModel;
    // *** End programmer edit section *** (Using statements)


    /// <summary>
    /// IServiceBusService.
    /// </summary>
    // *** Start programmer edit section *** (IServiceBusService CustomAttributes)
    [ServiceContract(Namespace = "http://tempuri.org/", ConfigurationName = "HighwaySbWcf.IServiceBusService")]
    // *** End programmer edit section *** (IServiceBusService CustomAttributes)
    public interface IServiceBusService
    {
        
        // *** Start programmer edit section *** (IServiceBusService.CreateClient System.String System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.CreateClient System.String System.String System.String CustomAttributes)
        void CreateClient([MessageParameter(Name = "clientId")] string clientId, [MessageParameter(Name = "name")] string name, [MessageParameter(Name = "address")] string address);
        
        // *** Start programmer edit section *** (IServiceBusService.DeleteClient System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.DeleteClient System.String CustomAttributes)
        void DeleteClient([MessageParameter(Name = "clientId")] string clientId);
        
        // *** Start programmer edit section *** (IServiceBusService.DoesEventRisen System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.DoesEventRisen System.String System.String CustomAttributes)
        bool DoesEventRisen([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "EventTypeID")] string eventTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.GetCurrentMessageCount System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetCurrentMessageCount System.String CustomAttributes)
        int GetCurrentMessageCount([MessageParameter(Name = "ClientID")] string clientId);
        
        // *** Start programmer edit section *** (IServiceBusService.GetCurrentThisTypeMessageCount System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetCurrentThisTypeMessageCount System.String System.String CustomAttributes)
        int GetCurrentThisTypeMessageCount([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "MessageTypeID")] string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.GetMessageFromESB System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageFromESB System.String System.String CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageFromESB GetMessageFromESB([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "MessageTypeID")] string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.GetMessageInfo System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageInfo System.String System.String CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageOrderingInformation GetMessageInfo([MessageParameter(Name = "clientId")] string clientId, [MessageParameter(Name = "messageTypeId")] string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.GetMessageWithGroupFromESB System.String System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageWithGroupFromESB System.String System.String System.String CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageFromESB GetMessageWithGroupFromESB([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "MessageTypeID")] string messageTypeId, [MessageParameter(Name = "groupName")] string groupName);
        
        // *** Start programmer edit section *** (IServiceBusService.GetMessageInfoWithGroup System.String System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageInfoWithGroup System.String System.String System.String CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageOrderingInformation GetMessageInfoWithGroup([MessageParameter(Name = "clientId")] string clientId, [MessageParameter(Name = "messageTypeId")] string messageTypeId, [MessageParameter(Name = "groupName")] string groupName);
        
        // *** Start programmer edit section *** (IServiceBusService.GetMessageWithTagsFromESB System.String System.String string[] CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageWithTagsFromESB System.String System.String string[] CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageFromESB GetMessageWithTagsFromESB([MessageParameter(Name = "clientId")] string clientId, [MessageParameter(Name = "messageTypeId")] string messageTypeId, [MessageParameter(Name = "tags")] string[] tags);

        // *** Start programmer edit section *** (IServiceBusService.GetMessageInfoWithTags System.String System.String string[] CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.GetMessageInfoWithTags System.String System.String string[] CustomAttributes)
        NewPlatform.Flexberry.ServiceBus.MessageOrderingInformation GetMessageInfoWithTags([MessageParameter(Name = "clientId")] string clientId, [MessageParameter(Name = "messageTypeId")] string messageTypeId, [MessageParameter(Name = "tags")] string[] tags);

        // *** Start programmer edit section *** (IServiceBusService.RiseEventOnESB System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.RiseEventOnESB System.String System.String CustomAttributes)
        void RiseEventOnESB([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "EventTypeID")] string eventTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.SendMessageToESB NewPlatform.Flexberry.ServiceBus.MessageForESB CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.SendMessageToESB NewPlatform.Flexberry.ServiceBus.MessageForESB CustomAttributes)
        void SendMessageToESB([MessageParameter(Name = "message")] NewPlatform.Flexberry.ServiceBus.MessageForESB message);
        
        // *** Start programmer edit section *** (IServiceBusService.SendMessageToESBWithUseGroup NewPlatform.Flexberry.ServiceBus.MessageForESB System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.SendMessageToESBWithUseGroup NewPlatform.Flexberry.ServiceBus.MessageForESB System.String CustomAttributes)
        void SendMessageToESBWithUseGroup([MessageParameter(Name = "message")] NewPlatform.Flexberry.ServiceBus.MessageForESB message, [MessageParameter(Name = "groupName")] string groupName);
        
        // *** Start programmer edit section *** (IServiceBusService.SubscribeClientForEventCallback System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.SubscribeClientForEventCallback System.String System.String CustomAttributes)
        void SubscribeClientForEventCallback([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "EventTypeID")] string eventTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.SubscribeClientForMessageCallback System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.SubscribeClientForMessageCallback System.String System.String CustomAttributes)
        void SubscribeClientForMessageCallback([MessageParameter(Name = "ClientID")] string clientId, [MessageParameter(Name = "MessageTypeID")] string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusService.IsUp CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusService.IsUp CustomAttributes)
        bool IsUp();
    }
}