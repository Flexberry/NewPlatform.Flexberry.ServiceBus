/*

Create tables.
Create user Administrator (login=admin, password=admin).
Create permissions for Administrator.

*/

CREATE TABLE [SubStatisticsMonitor] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Код] INT  NOT NULL,

	 [Категория] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [Подписка] UNIQUEIDENTIFIER  NOT NULL,

	 [StatisticsMonitor] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Шина] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [InteropАдрес] VARCHAR(255)  NOT NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [Ид] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NULL,

	 [Description] VARCHAR(MAX)  NULL,

	 [Адрес] VARCHAR(255)  NULL,

	 [DnsIdentity] VARCHAR(255)  NULL,

	 [ConnectionsLimit] INT  NULL,

	 [SequentialSent] BIT  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [StatRecord] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Since] DATETIME  NOT NULL,

	 [To] DATETIME  NOT NULL,

	 [StatInterval] VARCHAR(12)  NOT NULL,

	 [SentCount] INT  NULL,

	 [ReceivedCount] INT  NULL,

	 [ErrorsCount] INT  NULL,

	 [UniqueErrorsCount] INT  NULL,

	 [ConnectionCount] INT  NULL,

	 [QueueLength] INT  NULL,

	 [AvgTimeSent] INT  NULL,

	 [AvgTimeSql] INT  NULL,

	 [StatSetting] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [StatSetting] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [Подписка] UNIQUEIDENTIFIER  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Клиент] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Ид] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NULL,

	 [Description] VARCHAR(MAX)  NULL,

	 [Адрес] VARCHAR(255)  NULL,

	 [DnsIdentity] VARCHAR(255)  NULL,

	 [ConnectionsLimit] INT  NULL,

	 [SequentialSent] BIT  NOT NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [CompressionSetting] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [TargetCompression] VARCHAR(12)  NOT NULL,

	 [LifetimeLimit] INT  NOT NULL,

	 [LifetimeUnits] VARCHAR(6)  NOT NULL,

	 [Period] INT  NOT NULL,

	 [PeriodUnits] VARCHAR(6)  NOT NULL,

	 [NextCompressionTime] DATETIME  NOT NULL,

	 [LastCompressionTime] DATETIME  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [StatSetting] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Сообщение] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [ВремяСледующейОтправки] DATETIME  NOT NULL,

	 [ВремяФормирования] DATETIME  NOT NULL,

	 [Отправляется] BIT  NULL,

	 [FailsCount] INT  NULL,

	 [Отправитель] VARCHAR(255)  NULL,

	 [Тело] TEXT  NULL,

	 [ВложениеДляБазы] TEXT  NULL,

	 [Приоритет] INT  NULL,

	 [ИмяГруппы] VARCHAR(255)  NULL,

	 [Тэги] VARCHAR(MAX)  NULL,

	 [LogMessages] VARCHAR(MAX)  NULL,

	 [ТипСообщения_m0] UNIQUEIDENTIFIER  NOT NULL,

	 [Получатель_m0] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [OutboundMessageTypeRestriction] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [ТипСообщения] UNIQUEIDENTIFIER  NOT NULL,

	 [Клиент] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Подписка] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Описание] VARCHAR(MAX)  NULL,

	 [ExpiryDate] DATETIME  NOT NULL,

	 [IsCallback] BIT  NULL,

	 [ПередаватьПо] VARCHAR(4)  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 [ТипСообщения_m0] UNIQUEIDENTIFIER  NOT NULL,

	 [Клиент_m0] UNIQUEIDENTIFIER  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [StatisticsMonitor] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Логин] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NOT NULL,

	 [ДоступенДругимПользователям] BIT  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [ТипСообщения] (

	 [primaryKey] UNIQUEIDENTIFIER  NOT NULL,

	 [Ид] VARCHAR(255)  NULL,

	 [Наименование] VARCHAR(255)  NULL,

	 [Комментарий] VARCHAR(MAX)  NULL,

	 [CreateTime] DATETIME  NULL,

	 [Creator] VARCHAR(255)  NULL,

	 [EditTime] DATETIME  NULL,

	 [Editor] VARCHAR(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMNETLOCKDATA] (

	 [LockKey] VARCHAR(300)  NOT NULL,

	 [UserName] VARCHAR(300)  NOT NULL,

	 [LockDate] DATETIME  NULL,

	 PRIMARY KEY ([LockKey]))


CREATE TABLE [STORMSETTINGS] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Module] varchar(1000)  NULL,

	 [Name] varchar(255)  NULL,

	 [Value] text  NULL,

	 [User] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAdvLimit] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [User] varchar(255)  NULL,

	 [Published] bit  NULL,

	 [Module] varchar(255)  NULL,

	 [Name] varchar(255)  NULL,

	 [Value] text  NULL,

	 [HotKeyData] int  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERSETTING] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(255)  NOT NULL,

	 [DataObjectView] varchar(255)  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMWEBSEARCH] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(255)  NOT NULL,

	 [Order] INT  NOT NULL,

	 [PresentView] varchar(255)  NOT NULL,

	 [DetailedView] varchar(255)  NOT NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERDETAIL] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Caption] varchar(255)  NOT NULL,

	 [DataObjectView] varchar(255)  NOT NULL,

	 [ConnectMasterProp] varchar(255)  NOT NULL,

	 [OwnerConnectProp] varchar(255)  NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMFILTERLOOKUP] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [DataObjectType] varchar(255)  NOT NULL,

	 [Container] varchar(255)  NULL,

	 [ContainerTag] varchar(255)  NULL,

	 [FieldsToView] varchar(255)  NULL,

	 [FilterSetting_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [UserSetting] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [AppName] varchar(256)  NULL,

	 [UserName] varchar(512)  NULL,

	 [UserGuid] uniqueidentifier  NULL,

	 [ModuleName] varchar(1024)  NULL,

	 [ModuleGuid] uniqueidentifier  NULL,

	 [SettName] varchar(256)  NULL,

	 [SettGuid] uniqueidentifier  NULL,

	 [SettLastAccessTime] DATETIME  NULL,

	 [StrVal] varchar(256)  NULL,

	 [TxtVal] varchar(max)  NULL,

	 [IntVal] int  NULL,

	 [BoolVal] bit  NULL,

	 [GuidVal] uniqueidentifier  NULL,

	 [DecimalVal] decimal(20,10)  NULL,

	 [DateTimeVal] DATETIME  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [ApplicationLog] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Category] varchar(64)  NULL,

	 [EventId] INT  NULL,

	 [Priority] INT  NULL,

	 [Severity] varchar(32)  NULL,

	 [Title] varchar(256)  NULL,

	 [Timestamp] DATETIME  NULL,

	 [MachineName] varchar(32)  NULL,

	 [AppDomainName] varchar(512)  NULL,

	 [ProcessId] varchar(256)  NULL,

	 [ProcessName] varchar(512)  NULL,

	 [ThreadName] varchar(512)  NULL,

	 [Win32ThreadId] varchar(128)  NULL,

	 [Message] varchar(2500)  NULL,

	 [FormattedMessage] varchar(max)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAG] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(80)  NOT NULL,

	 [Login] varchar(50)  NULL,

	 [Pwd] varchar(50)  NULL,

	 [IsUser] bit  NOT NULL,

	 [IsGroup] bit  NOT NULL,

	 [IsRole] bit  NOT NULL,

	 [ConnString] varchar(255)  NULL,

	 [Enabled] bit  NULL,

	 [Email] varchar(80)  NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLG] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Group_m0] uniqueidentifier  NOT NULL,

	 [User_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuObjType] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] nvarchar(255)  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuEntity] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [ObjectPrimaryKey] nvarchar(38)  NOT NULL,

	 [OperationTime] DATETIME  NOT NULL,

	 [OperationType] nvarchar(100)  NOT NULL,

	 [ExecutionResult] nvarchar(12)  NOT NULL,

	 [Source] nvarchar(255)  NOT NULL,

	 [SerializedField] nvarchar(max)  NULL,

	 [User_m0] uniqueidentifier  NOT NULL,

	 [ObjectType_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAuField] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Field] nvarchar(100)  NOT NULL,

	 [OldValue] nvarchar(max)  NULL,

	 [NewValue] nvarchar(max)  NULL,

	 [MainChange_m0] uniqueidentifier  NULL,

	 [AuditEntity_m0] uniqueidentifier  NOT NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMI] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [User_m0] uniqueidentifier  NOT NULL,

	 [Agent_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [Session] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [UserKey] uniqueidentifier  NULL,

	 [StartedAt] datetime  NULL,

	 [LastAccess] datetime  NULL,

	 [Closed] bit  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMS] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Name] varchar(100)  NOT NULL,

	 [Type] varchar(100)  NULL,

	 [IsAttribute] bit  NOT NULL,

	 [IsOperation] bit  NOT NULL,

	 [IsView] bit  NOT NULL,

	 [IsClass] bit  NOT NULL,

	 [SharedOper] bit  NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMP] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Subject_m0] uniqueidentifier  NOT NULL,

	 [Agent_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMF] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [FilterText] varchar(MAX)  NULL,

	 [Name] varchar(255)  NULL,

	 [FilterTypeNView] varchar(255)  NULL,

	 [Subject_m0] uniqueidentifier  NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMAC] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [TypeAccess] varchar(7)  NULL,

	 [Filter_m0] uniqueidentifier  NULL,

	 [Permition_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLO] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Class_m0] uniqueidentifier  NOT NULL,

	 [Operation_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLA] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [View_m0] uniqueidentifier  NOT NULL,

	 [Attribute_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLV] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [Class_m0] uniqueidentifier  NOT NULL,

	 [View_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))


CREATE TABLE [STORMLR] (

	 [primaryKey] uniqueidentifier  NOT NULL,

	 [StartDate] datetime  NULL,

	 [EndDate] datetime  NULL,

	 [Agent_m0] uniqueidentifier  NOT NULL,

	 [Role_m0] uniqueidentifier  NOT NULL,

	 [CreateTime] datetime  NULL,

	 [Creator] varchar(255)  NULL,

	 [EditTime] datetime  NULL,

	 [Editor] varchar(255)  NULL,

	 PRIMARY KEY ([primaryKey]))




 ALTER TABLE [SubStatisticsMonitor] ADD CONSTRAINT [SubStatisticsMonitor_FПодписка_0] FOREIGN KEY ([Подписка]) REFERENCES [Подписка]
CREATE INDEX SubStatisticsMonitor_IПодписка on [SubStatisticsMonitor] ([Подписка])

 ALTER TABLE [SubStatisticsMonitor] ADD CONSTRAINT [SubStatisticsMonitor_FStatisticsMonitor_0] FOREIGN KEY ([StatisticsMonitor]) REFERENCES [StatisticsMonitor]
CREATE INDEX SubStatisticsMonitor_IStatisticsMonitor on [SubStatisticsMonitor] ([StatisticsMonitor])

 ALTER TABLE [StatRecord] ADD CONSTRAINT [StatRecord_FStatSetting_0] FOREIGN KEY ([StatSetting]) REFERENCES [StatSetting]
CREATE INDEX StatRecord_IStatSetting on [StatRecord] ([StatSetting])

 ALTER TABLE [StatSetting] ADD CONSTRAINT [StatSetting_FПодписка_0] FOREIGN KEY ([Подписка]) REFERENCES [Подписка]
CREATE INDEX StatSetting_IПодписка on [StatSetting] ([Подписка])

 ALTER TABLE [CompressionSetting] ADD CONSTRAINT [CompressionSetting_FStatSetting_0] FOREIGN KEY ([StatSetting]) REFERENCES [StatSetting]
CREATE INDEX CompressionSetting_IStatSetting on [CompressionSetting] ([StatSetting])

 ALTER TABLE [Сообщение] ADD CONSTRAINT [Сообщение_FТипСообщения_0] FOREIGN KEY ([ТипСообщения_m0]) REFERENCES [ТипСообщения]
CREATE INDEX Сообщение_IТипСообщения_m0 on [Сообщение] ([ТипСообщения_m0])

 ALTER TABLE [Сообщение] ADD CONSTRAINT [Сообщение_FКлиент_0] FOREIGN KEY ([Получатель_m0]) REFERENCES [Клиент]
CREATE INDEX Сообщение_IПолучатель_m0 on [Сообщение] ([Получатель_m0])

 ALTER TABLE [OutboundMessageTypeRestriction] ADD CONSTRAINT [OutboundMessageTypeRestriction_FТипСообщения_0] FOREIGN KEY ([ТипСообщения]) REFERENCES [ТипСообщения]
CREATE INDEX OutboundMessageTypeRestriction_IТипСообщения on [OutboundMessageTypeRestriction] ([ТипСообщения])

 ALTER TABLE [OutboundMessageTypeRestriction] ADD CONSTRAINT [OutboundMessageTypeRestriction_FКлиент_0] FOREIGN KEY ([Клиент]) REFERENCES [Клиент]
CREATE INDEX OutboundMessageTypeRestriction_IКлиент on [OutboundMessageTypeRestriction] ([Клиент])

 ALTER TABLE [Подписка] ADD CONSTRAINT [Подписка_FТипСообщения_0] FOREIGN KEY ([ТипСообщения_m0]) REFERENCES [ТипСообщения]
CREATE INDEX Подписка_IТипСообщения_m0 on [Подписка] ([ТипСообщения_m0])

 ALTER TABLE [Подписка] ADD CONSTRAINT [Подписка_FКлиент_0] FOREIGN KEY ([Клиент_m0]) REFERENCES [Клиент]
CREATE INDEX Подписка_IКлиент_m0 on [Подписка] ([Клиент_m0])

 ALTER TABLE [STORMWEBSEARCH] ADD CONSTRAINT [STORMWEBSEARCH_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMFILTERDETAIL] ADD CONSTRAINT [STORMFILTERDETAIL_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMFILTERLOOKUP] ADD CONSTRAINT [STORMFILTERLOOKUP_FSTORMFILTERSETTING_0] FOREIGN KEY ([FilterSetting_m0]) REFERENCES [STORMFILTERSETTING]

 ALTER TABLE [STORMLG] ADD CONSTRAINT [STORMLG_FSTORMAG_0] FOREIGN KEY ([Group_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMLG] ADD CONSTRAINT [STORMLG_FSTORMAG_1] FOREIGN KEY ([User_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMAuEntity] ADD CONSTRAINT [STORMAuEntity_FSTORMAG_0] FOREIGN KEY ([User_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMAuEntity] ADD CONSTRAINT [STORMAuEntity_FSTORMAuObjType_0] FOREIGN KEY ([ObjectType_m0]) REFERENCES [STORMAuObjType]

 ALTER TABLE [STORMAuField] ADD CONSTRAINT [STORMAuField_FSTORMAuField_0] FOREIGN KEY ([MainChange_m0]) REFERENCES [STORMAuField]

 ALTER TABLE [STORMAuField] ADD CONSTRAINT [STORMAuField_FSTORMAuEntity_0] FOREIGN KEY ([AuditEntity_m0]) REFERENCES [STORMAuEntity]

 ALTER TABLE [STORMI] ADD CONSTRAINT [STORMI_FSTORMAG_0] FOREIGN KEY ([User_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMI] ADD CONSTRAINT [STORMI_FSTORMAG_1] FOREIGN KEY ([Agent_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMP] ADD CONSTRAINT [STORMP_FSTORMS_0] FOREIGN KEY ([Subject_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMP] ADD CONSTRAINT [STORMP_FSTORMAG_0] FOREIGN KEY ([Agent_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMF] ADD CONSTRAINT [STORMF_FSTORMS_0] FOREIGN KEY ([Subject_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMAC] ADD CONSTRAINT [STORMAC_FSTORMF_0] FOREIGN KEY ([Filter_m0]) REFERENCES [STORMF]

 ALTER TABLE [STORMAC] ADD CONSTRAINT [STORMAC_FSTORMP_0] FOREIGN KEY ([Permition_m0]) REFERENCES [STORMP]

 ALTER TABLE [STORMLO] ADD CONSTRAINT [STORMLO_FSTORMS_0] FOREIGN KEY ([Class_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLO] ADD CONSTRAINT [STORMLO_FSTORMS_1] FOREIGN KEY ([Operation_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLA] ADD CONSTRAINT [STORMLA_FSTORMS_0] FOREIGN KEY ([View_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLA] ADD CONSTRAINT [STORMLA_FSTORMS_1] FOREIGN KEY ([Attribute_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLV] ADD CONSTRAINT [STORMLV_FSTORMS_0] FOREIGN KEY ([Class_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLV] ADD CONSTRAINT [STORMLV_FSTORMS_1] FOREIGN KEY ([View_m0]) REFERENCES [STORMS]

 ALTER TABLE [STORMLR] ADD CONSTRAINT [STORMLR_FSTORMAG_0] FOREIGN KEY ([Agent_m0]) REFERENCES [STORMAG]

 ALTER TABLE [STORMLR] ADD CONSTRAINT [STORMLR_FSTORMAG_1] FOREIGN KEY ([Role_m0]) REFERENCES [STORMAG]



 INSERT INTO [STORMAG]([primaryKey], [Name], [Login], [Pwd], [IsUser], [IsGroup], [IsRole], [Enabled])
	VALUES (NEWID(), 'Administrator', 'admin', 'D033E22AE348AEB5660FC2140AEC35850C4DA997', 1, 0, 0, 1)
	, (NEWID(), 'admin', NULL, NULL, 0, 0, 1, 1);

INSERT INTO [STORMLR]([primaryKey], [Agent_m0], [Role_m0])
	VALUES (NEWID(), (SELECT [primaryKey] FROM [STORMAG] WHERE [Name] = 'Administrator'), (SELECT [primaryKey] FROM [STORMAG] WHERE [Name] = 'admin'));

INSERT INTO [STORMS]([primaryKey], [Name], [IsAttribute], [IsOperation], [IsView], [IsClass], [SharedOper])
	VALUES (NEWID(), 'NewPlatform.Flexberry.ServiceBus.SendingPermission', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.MessageType', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.Subscription', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.StatisticsCompressionSetting', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.StatisticsSetting', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.SubscriptionStatisticsMonitor', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.Client', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.Message', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.StatisticsMonitor', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.StatisticsRecord', 0, 0, 0, 1, 1)
	, (NEWID(), 'NewPlatform.Flexberry.ServiceBus.Bus', 0, 0, 0, 1, 1);

INSERT INTO [STORMP]([primaryKey], [Subject_m0], [Agent_m0])
	SELECT NEWID(), [primaryKey], (SELECT [primaryKey] FROM [STORMAG] WHERE [Name] = 'admin') FROM [STORMS];

INSERT INTO [STORMAC]([primaryKey], [TypeAccess], [Permition_m0])
	SELECT NEWID(), 'Full', [primaryKey] FROM [STORMP];
