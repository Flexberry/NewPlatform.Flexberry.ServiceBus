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
    /// IServiceBusManager.
    /// </summary>
    // *** Start programmer edit section *** (IServiceBusManager CustomAttributes)
    [ServiceContract(Namespace = "http://tempuri.org/", ConfigurationName = "FlexberryServiceBus.IServiceBusManager")]
    // *** End programmer edit section *** (IServiceBusManager CustomAttributes)
    internal interface IServiceBusManager
    {
        
        // *** Start programmer edit section *** (IServiceBusManager CustomMembers)

        // *** End programmer edit section *** (IServiceBusManager CustomMembers)

        
        // *** Start programmer edit section *** (IServiceBusManager.DeleteClient System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.DeleteClient System.String CustomAttributes)
        void DeleteClient(string clientId);
        
        // *** Start programmer edit section *** (IServiceBusManager.CreateMessageType NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.CreateMessageType NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType CustomAttributes)
        void CreateMessageType(NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType messageType);
        
        // *** Start programmer edit section *** (IServiceBusManager.UpdateMessageType System.String NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.UpdateMessageType System.String NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType CustomAttributes)
        void UpdateMessageType(string messageTypeId, NewPlatform.Flexberry.ServiceBus.ServiceBusMessageType messageType);
        
        // *** Start programmer edit section *** (IServiceBusManager.DeleteMessageType System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.DeleteMessageType System.String CustomAttributes)
        void DeleteMessageType(string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusManager.GetMessageTypes CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.GetMessageTypes CustomAttributes)
        System.Collections.Generic.IEnumerable<ServiceBusMessageType> GetMessageTypes();
        
        // *** Start programmer edit section *** (IServiceBusManager.CreateClient NewPlatform.Flexberry.ServiceBus.ServiceBusClient CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.CreateClient NewPlatform.Flexberry.ServiceBus.ServiceBusClient CustomAttributes)
        void CreateClient(NewPlatform.Flexberry.ServiceBus.ServiceBusClient client);
        
        // *** Start programmer edit section *** (IServiceBusManager.UpdateClient System.String NewPlatform.Flexberry.ServiceBus.ServiceBusClient CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.UpdateClient System.String NewPlatform.Flexberry.ServiceBus.ServiceBusClient CustomAttributes)
        void UpdateClient(string clientId, NewPlatform.Flexberry.ServiceBus.ServiceBusClient client);
        
        // *** Start programmer edit section *** (IServiceBusManager.GetClients CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.GetClients CustomAttributes)
        System.Collections.Generic.IEnumerable<ServiceBusClient> GetClients();
        
        // *** Start programmer edit section *** (IServiceBusManager.CreateSubscription NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.CreateSubscription NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription CustomAttributes)
        void CreateSubscription(NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription subscription);
        
        // *** Start programmer edit section *** (IServiceBusManager.UpdateSubscription System.String NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.UpdateSubscription System.String NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription CustomAttributes)
        void UpdateSubscription(string subscriptionId, NewPlatform.Flexberry.ServiceBus.ServiceBusSubscription subscription);
        
        // *** Start programmer edit section *** (IServiceBusManager.DeleteSubscription System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.DeleteSubscription System.String CustomAttributes)
        void DeleteSubscription(string subscriptionId);
        
        // *** Start programmer edit section *** (IServiceBusManager.GetSubscriptions System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.GetSubscriptions System.String CustomAttributes)
        System.Collections.Generic.IEnumerable<ServiceBusSubscription> GetSubscriptions(string clientId);
        
        // *** Start programmer edit section *** (IServiceBusManager.CreateSendingPermission System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.CreateSendingPermission System.String System.String CustomAttributes)
        void CreateSendingPermission(string clientId, string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusManager.DeleteSendingPermission System.String System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.DeleteSendingPermission System.String System.String CustomAttributes)
        void DeleteSendingPermission(string clientId, string messageTypeId);
        
        // *** Start programmer edit section *** (IServiceBusManager.GetSendingPermissions System.String CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.GetSendingPermissions System.String CustomAttributes)
        string[] GetSendingPermissions(string clientId);
        
        // *** Start programmer edit section *** (IServiceBusManager.GetCurrentState CustomAttributes)
        [OperationContract]
        // *** End programmer edit section *** (IServiceBusManager.GetCurrentState CustomAttributes)
        MessageInfo[] GetCurrentState();
    }
}
