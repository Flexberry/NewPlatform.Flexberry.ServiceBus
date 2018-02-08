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
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business.Audit;
    using ICSSoft.STORMNET.Business.Audit.Objects;


    // *** Start programmer edit section *** (Using statements)

    // *** End programmer edit section *** (Using statements)


    /// <summary>
    /// Client.
    /// </summary>
    // *** Start programmer edit section *** (Client CustomAttributes)

    // *** End programmer edit section *** (Client CustomAttributes)
    [ClassStorage("Клиент")]
    [AutoAltered()]
    [AccessType(ICSSoft.STORMNET.AccessType.@this)]
    [View("AuditView", new string[] {
            "ID as \'ID\'",
            "Name as \'Name\'",
            "Address as \'Address\'",
            "DnsIdentity as \'Dns identity\'",
            "Description as \'Description\'"})]
    [AssociatedDetailViewAttribute("AuditView", "SendingPermissions", "AuditView", true, "", "Sending permissions", true, new string[] {
            ""})]
    [AssociatedDetailViewAttribute("AuditView", "Subscriptions", "AuditView", true, "", "Subscriptions", true, new string[] {
            ""})]
    [View("EditView", new string[] {
            "ID as \'ID\'",
            "Name as \'Name\'",
            "Address as \'Address\'",
            "DnsIdentity as \'DNS Identity\'",
            "Description as \'Description\'"})]
    [AssociatedDetailViewAttribute("EditView", "Subscriptions", "DetailView", true, "", "Subscriptions", true, new string[] {
            ""})]
    [AssociatedDetailViewAttribute("EditView", "SendingPermissions", "DetailView", true, "", "Sending permissions", true, new string[] {
            ""})]
    [View("ListView", new string[] {
            "ID as \'ID\'",
            "Name as \'Name\'",
            "Address as \'Address\'"})]
    [View("LookupView", new string[] {
            "ID as \'ID\'",
            "Name as \'Name\'"})]
    public class Client : ICSSoft.STORMNET.DataObject, IDataObjectWithAuditFields
    {

        private string fID;

        private string fName;

        private string fAddress;

        private string fDnsIdentity;

        private string fDescription;

        private System.Nullable<System.DateTime> fCreateTime;

        private string fCreator;

        private System.Nullable<System.DateTime> fEditTime;

        private string fEditor;

        private NewPlatform.Flexberry.ServiceBus.DetailArrayOfSendingPermission fSendingPermissions;

        private NewPlatform.Flexberry.ServiceBus.DetailArrayOfSubscription fSubscriptions;

        // *** Start programmer edit section *** (Client CustomMembers)

        // *** End programmer edit section *** (Client CustomMembers)


        /// <summary>
        /// ID.
        /// </summary>
        // *** Start programmer edit section *** (Client.ID CustomAttributes)

        // *** End programmer edit section *** (Client.ID CustomAttributes)
        [PropertyStorage("Ид")]
        [StrLen(255)]
        public virtual string ID
        {
            get
            {
                // *** Start programmer edit section *** (Client.ID Get start)

                // *** End programmer edit section *** (Client.ID Get start)
                string result = this.fID;
                // *** Start programmer edit section *** (Client.ID Get end)
                if (string.IsNullOrEmpty(result))
                    result = __PrimaryKey.ToString();
                // *** End programmer edit section *** (Client.ID Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.ID Set start)

                // *** End programmer edit section *** (Client.ID Set start)
                this.fID = value;
                // *** Start programmer edit section *** (Client.ID Set end)

                // *** End programmer edit section *** (Client.ID Set end)
            }
        }

        /// <summary>
        /// Name.
        /// </summary>
        // *** Start programmer edit section *** (Client.Name CustomAttributes)

        // *** End programmer edit section *** (Client.Name CustomAttributes)
        [PropertyStorage("Наименование")]
        [StrLen(255)]
        public virtual string Name
        {
            get
            {
                // *** Start programmer edit section *** (Client.Name Get start)

                // *** End programmer edit section *** (Client.Name Get start)
                string result = this.fName;
                // *** Start programmer edit section *** (Client.Name Get end)

                // *** End programmer edit section *** (Client.Name Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Name Set start)

                // *** End programmer edit section *** (Client.Name Set start)
                this.fName = value;
                // *** Start programmer edit section *** (Client.Name Set end)

                // *** End programmer edit section *** (Client.Name Set end)
            }
        }

        /// <summary>
        /// Address.
        /// </summary>
        // *** Start programmer edit section *** (Client.Address CustomAttributes)

        // *** End programmer edit section *** (Client.Address CustomAttributes)
        [PropertyStorage("Адрес")]
        [StrLen(255)]
        public virtual string Address
        {
            get
            {
                // *** Start programmer edit section *** (Client.Address Get start)

                // *** End programmer edit section *** (Client.Address Get start)
                string result = this.fAddress;
                // *** Start programmer edit section *** (Client.Address Get end)

                // *** End programmer edit section *** (Client.Address Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Address Set start)

                // *** End programmer edit section *** (Client.Address Set start)
                this.fAddress = value;
                // *** Start programmer edit section *** (Client.Address Set end)

                // *** End programmer edit section *** (Client.Address Set end)
            }
        }

        /// <summary>
        /// DnsIdentity.
        /// </summary>
        // *** Start programmer edit section *** (Client.DnsIdentity CustomAttributes)

        // *** End programmer edit section *** (Client.DnsIdentity CustomAttributes)
        [StrLen(255)]
        public virtual string DnsIdentity
        {
            get
            {
                // *** Start programmer edit section *** (Client.DnsIdentity Get start)

                // *** End programmer edit section *** (Client.DnsIdentity Get start)
                string result = this.fDnsIdentity;
                // *** Start programmer edit section *** (Client.DnsIdentity Get end)

                // *** End programmer edit section *** (Client.DnsIdentity Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.DnsIdentity Set start)

                // *** End programmer edit section *** (Client.DnsIdentity Set start)
                this.fDnsIdentity = value;
                // *** Start programmer edit section *** (Client.DnsIdentity Set end)

                // *** End programmer edit section *** (Client.DnsIdentity Set end)
            }
        }

        /// <summary>
        /// Description.
        /// </summary>
        // *** Start programmer edit section *** (Client.Description CustomAttributes)

        // *** End programmer edit section *** (Client.Description CustomAttributes)
        public virtual string Description
        {
            get
            {
                // *** Start programmer edit section *** (Client.Description Get start)

                // *** End programmer edit section *** (Client.Description Get start)
                string result = this.fDescription;
                // *** Start programmer edit section *** (Client.Description Get end)

                // *** End programmer edit section *** (Client.Description Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Description Set start)

                // *** End programmer edit section *** (Client.Description Set start)
                this.fDescription = value;
                // *** Start programmer edit section *** (Client.Description Set end)

                // *** End programmer edit section *** (Client.Description Set end)
            }
        }

        /// <summary>
        /// Время создания объекта.
        /// </summary>
        // *** Start programmer edit section *** (Client.CreateTime CustomAttributes)

        // *** End programmer edit section *** (Client.CreateTime CustomAttributes)
        public virtual System.Nullable<System.DateTime> CreateTime
        {
            get
            {
                // *** Start programmer edit section *** (Client.CreateTime Get start)

                // *** End programmer edit section *** (Client.CreateTime Get start)
                System.Nullable<System.DateTime> result = this.fCreateTime;
                // *** Start programmer edit section *** (Client.CreateTime Get end)

                // *** End programmer edit section *** (Client.CreateTime Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.CreateTime Set start)

                // *** End programmer edit section *** (Client.CreateTime Set start)
                this.fCreateTime = value;
                // *** Start programmer edit section *** (Client.CreateTime Set end)

                // *** End programmer edit section *** (Client.CreateTime Set end)
            }
        }

        /// <summary>
        /// Создатель объекта.
        /// </summary>
        // *** Start programmer edit section *** (Client.Creator CustomAttributes)

        // *** End programmer edit section *** (Client.Creator CustomAttributes)
        [StrLen(255)]
        public virtual string Creator
        {
            get
            {
                // *** Start programmer edit section *** (Client.Creator Get start)

                // *** End programmer edit section *** (Client.Creator Get start)
                string result = this.fCreator;
                // *** Start programmer edit section *** (Client.Creator Get end)

                // *** End programmer edit section *** (Client.Creator Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Creator Set start)

                // *** End programmer edit section *** (Client.Creator Set start)
                this.fCreator = value;
                // *** Start programmer edit section *** (Client.Creator Set end)

                // *** End programmer edit section *** (Client.Creator Set end)
            }
        }

        /// <summary>
        /// Время последнего редактирования объекта.
        /// </summary>
        // *** Start programmer edit section *** (Client.EditTime CustomAttributes)

        // *** End programmer edit section *** (Client.EditTime CustomAttributes)
        public virtual System.Nullable<System.DateTime> EditTime
        {
            get
            {
                // *** Start programmer edit section *** (Client.EditTime Get start)

                // *** End programmer edit section *** (Client.EditTime Get start)
                System.Nullable<System.DateTime> result = this.fEditTime;
                // *** Start programmer edit section *** (Client.EditTime Get end)

                // *** End programmer edit section *** (Client.EditTime Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.EditTime Set start)

                // *** End programmer edit section *** (Client.EditTime Set start)
                this.fEditTime = value;
                // *** Start programmer edit section *** (Client.EditTime Set end)

                // *** End programmer edit section *** (Client.EditTime Set end)
            }
        }

        /// <summary>
        /// Последний редактор объекта.
        /// </summary>
        // *** Start programmer edit section *** (Client.Editor CustomAttributes)

        // *** End programmer edit section *** (Client.Editor CustomAttributes)
        [StrLen(255)]
        public virtual string Editor
        {
            get
            {
                // *** Start programmer edit section *** (Client.Editor Get start)

                // *** End programmer edit section *** (Client.Editor Get start)
                string result = this.fEditor;
                // *** Start programmer edit section *** (Client.Editor Get end)

                // *** End programmer edit section *** (Client.Editor Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Editor Set start)

                // *** End programmer edit section *** (Client.Editor Set start)
                this.fEditor = value;
                // *** Start programmer edit section *** (Client.Editor Set end)

                // *** End programmer edit section *** (Client.Editor Set end)
            }
        }

        /// <summary>
        /// Client.
        /// </summary>
        // *** Start programmer edit section *** (Client.SendingPermissions CustomAttributes)

        // *** End programmer edit section *** (Client.SendingPermissions CustomAttributes)
        public virtual NewPlatform.Flexberry.ServiceBus.DetailArrayOfSendingPermission SendingPermissions
        {
            get
            {
                // *** Start programmer edit section *** (Client.SendingPermissions Get start)

                // *** End programmer edit section *** (Client.SendingPermissions Get start)
                if ((this.fSendingPermissions == null))
                {
                    this.fSendingPermissions = new NewPlatform.Flexberry.ServiceBus.DetailArrayOfSendingPermission(this);
                }
                NewPlatform.Flexberry.ServiceBus.DetailArrayOfSendingPermission result = this.fSendingPermissions;
                // *** Start programmer edit section *** (Client.SendingPermissions Get end)

                // *** End programmer edit section *** (Client.SendingPermissions Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.SendingPermissions Set start)

                // *** End programmer edit section *** (Client.SendingPermissions Set start)
                this.fSendingPermissions = value;
                // *** Start programmer edit section *** (Client.SendingPermissions Set end)

                // *** End programmer edit section *** (Client.SendingPermissions Set end)
            }
        }

        /// <summary>
        /// Client.
        /// </summary>
        // *** Start programmer edit section *** (Client.Subscriptions CustomAttributes)

        // *** End programmer edit section *** (Client.Subscriptions CustomAttributes)
        public virtual NewPlatform.Flexberry.ServiceBus.DetailArrayOfSubscription Subscriptions
        {
            get
            {
                // *** Start programmer edit section *** (Client.Subscriptions Get start)

                // *** End programmer edit section *** (Client.Subscriptions Get start)
                if ((this.fSubscriptions == null))
                {
                    this.fSubscriptions = new NewPlatform.Flexberry.ServiceBus.DetailArrayOfSubscription(this);
                }
                NewPlatform.Flexberry.ServiceBus.DetailArrayOfSubscription result = this.fSubscriptions;
                // *** Start programmer edit section *** (Client.Subscriptions Get end)

                // *** End programmer edit section *** (Client.Subscriptions Get end)
                return result;
            }
            set
            {
                // *** Start programmer edit section *** (Client.Subscriptions Set start)

                // *** End programmer edit section *** (Client.Subscriptions Set start)
                this.fSubscriptions = value;
                // *** Start programmer edit section *** (Client.Subscriptions Set end)

                // *** End programmer edit section *** (Client.Subscriptions Set end)
            }
        }

        /// <summary>
        /// Class views container.
        /// </summary>
        public class Views
        {

            /// <summary>
            /// "AuditView" view.
            /// </summary>
            public static ICSSoft.STORMNET.View AuditView
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("AuditView", typeof(NewPlatform.Flexberry.ServiceBus.Client));
                }
            }

            /// <summary>
            /// "EditView" view.
            /// </summary>
            public static ICSSoft.STORMNET.View EditView
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("EditView", typeof(NewPlatform.Flexberry.ServiceBus.Client));
                }
            }

            /// <summary>
            /// "ListView" view.
            /// </summary>
            public static ICSSoft.STORMNET.View ListView
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("ListView", typeof(NewPlatform.Flexberry.ServiceBus.Client));
                }
            }

            /// <summary>
            /// "LookupView" view.
            /// </summary>
            public static ICSSoft.STORMNET.View LookupView
            {
                get
                {
                    return ICSSoft.STORMNET.Information.GetView("LookupView", typeof(NewPlatform.Flexberry.ServiceBus.Client));
                }
            }
        }

        /// <summary>
        /// Audit class settings.
        /// </summary>
        public class AuditSettings
        {

            /// <summary>
            /// Включён ли аудит для класса.
            /// </summary>
            public static bool AuditEnabled = true;

            /// <summary>
            /// Использовать имя представления для аудита по умолчанию.
            /// </summary>
            public static bool UseDefaultView = false;

            /// <summary>
            /// Включён ли аудит операции чтения.
            /// </summary>
            public static bool SelectAudit = false;

            /// <summary>
            /// Имя представления для аудирования операции чтения.
            /// </summary>
            public static string SelectAuditViewName = "AuditView";

            /// <summary>
            /// Включён ли аудит операции создания.
            /// </summary>
            public static bool InsertAudit = true;

            /// <summary>
            /// Имя представления для аудирования операции создания.
            /// </summary>
            public static string InsertAuditViewName = "AuditView";

            /// <summary>
            /// Включён ли аудит операции изменения.
            /// </summary>
            public static bool UpdateAudit = true;

            /// <summary>
            /// Имя представления для аудирования операции изменения.
            /// </summary>
            public static string UpdateAuditViewName = "AuditView";

            /// <summary>
            /// Включён ли аудит операции удаления.
            /// </summary>
            public static bool DeleteAudit = true;

            /// <summary>
            /// Имя представления для аудирования операции удаления.
            /// </summary>
            public static string DeleteAuditViewName = "AuditView";

            /// <summary>
            /// Путь к форме просмотра результатов аудита.
            /// </summary>
            public static string FormUrl = "";

            /// <summary>
            /// Режим записи данных аудита (синхронный или асинхронный).
            /// </summary>
            public static ICSSoft.STORMNET.Business.Audit.Objects.tWriteMode WriteMode = ICSSoft.STORMNET.Business.Audit.Objects.tWriteMode.Synchronous;

            /// <summary>
            /// Максимальная длина сохраняемого значения поля (если 0, то строка обрезаться не будет).
            /// </summary>
            public static int PrunningLength = 0;

            /// <summary>
            /// Показывать ли пользователям в изменениях первичные ключи.
            /// </summary>
            public static bool ShowPrimaryKey = false;

            /// <summary>
            /// Сохранять ли старое значение.
            /// </summary>
            public static bool KeepOldValue = true;

            /// <summary>
            /// Сжимать ли сохраняемые значения.
            /// </summary>
            public static bool Compress = false;

            /// <summary>
            /// Сохранять ли все значения атрибутов, а не только изменяемые.
            /// </summary>
            public static bool KeepAllValues = false;
        }
    }
}
