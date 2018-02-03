/*

Create tables.
Create user Administrator (login=admin, password=admin).
Create permissions for Administrator.

*/

CREATE TABLE SubscriptionStatisticsMonitor (

 primaryKey UUID NOT NULL,

 Number INT NOT NULL,

 Category VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Subscription UUID NOT NULL,

 StatisticsMonitor UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Bus (

 primaryKey UUID NOT NULL,

 ManagerAddress VARCHAR(255) NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 ID VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 Address VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 Description TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsRecord (

 primaryKey UUID NOT NULL,

 Since TIMESTAMP(3) NOT NULL,

 "To" TIMESTAMP(3) NOT NULL,

 StatisticsInterval VARCHAR(12) NOT NULL,

 SentCount INT NULL,

 ReceivedCount INT NULL,

 ErrorsCount INT NULL,

 UniqueErrorsCount INT NULL,

 ConnectionCount INT NULL,

 QueueLength INT NULL,

 SentAvgTime INT NULL,

 QueryAvgTime INT NULL,

 StatisticsSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsSetting (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Subscription UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Client (

 primaryKey UUID NOT NULL,

 ID VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 Address VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 Description TEXT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsCompressionSetting (

 primaryKey UUID NOT NULL,

 CompressTo VARCHAR(12) NOT NULL,

 StatisticsAgeCount INT NOT NULL,

 StatisticsAgeUnits VARCHAR(6) NOT NULL,

 CompressFrequencyCount INT NOT NULL,

 CompressFrequencyUnits VARCHAR(6) NOT NULL,

 NextCompressTime TIMESTAMP(3) NOT NULL,

 LastCompressTime TIMESTAMP(3) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 StatisticsSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Message (

 primaryKey UUID NOT NULL,

 SendingTime TIMESTAMP(3) NOT NULL,

 ReceivingTime TIMESTAMP(3) NOT NULL,

 IsSending BOOLEAN NULL,

 ErrorCount INT NULL,

 Sender VARCHAR(255) NULL,

 Body TEXT NULL,

 Attachment TEXT NULL,

 Priority INT NULL,

 "Group" VARCHAR(255) NULL,

 Tags VARCHAR NULL,

 Logs VARCHAR NULL,

 MessageType UUID NOT NULL,

 Recipient UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE SendingPermission (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 MessageType UUID NOT NULL,

 Client UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Subscription (

 primaryKey UUID NOT NULL,

 Description TEXT NULL,

 ExpiryDate TIMESTAMP(3) NOT NULL,

 IsCallback BOOLEAN NULL,

 TransportType VARCHAR(4) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 MessageType UUID NOT NULL,

 Client UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsMonitor (

 primaryKey UUID NOT NULL,

 Owner VARCHAR(255) NULL,

 Name VARCHAR(255) NOT NULL,

 Public BOOLEAN NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE MessageType (

 primaryKey UUID NOT NULL,

 ID VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 Description TEXT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMNETLOCKDATA (

 LockKey VARCHAR(300) NOT NULL,

 UserName VARCHAR(300) NOT NULL,

 LockDate TIMESTAMP(3) NULL,

 PRIMARY KEY (LockKey));


CREATE TABLE STORMSETTINGS (

 primaryKey UUID NOT NULL,

 Module VARCHAR(1000) NULL,

 Name VARCHAR(255) NULL,

 Value TEXT NULL,

 "User" VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAdvLimit (

 primaryKey UUID NOT NULL,

 "User" VARCHAR(255) NULL,

 Published BOOLEAN NULL,

 Module VARCHAR(255) NULL,

 Name VARCHAR(255) NULL,

 Value TEXT NULL,

 HotKeyData INT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERSETTING (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 DataObjectView VARCHAR(255) NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMWEBSEARCH (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 "Order" INT NOT NULL,

 PresentView VARCHAR(255) NOT NULL,

 DetailedView VARCHAR(255) NOT NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERDETAIL (

 primaryKey UUID NOT NULL,

 Caption VARCHAR(255) NOT NULL,

 DataObjectView VARCHAR(255) NOT NULL,

 ConnectMasterProp VARCHAR(255) NOT NULL,

 OwnerConnectProp VARCHAR(255) NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMFILTERLOOKUP (

 primaryKey UUID NOT NULL,

 DataObjectType VARCHAR(255) NOT NULL,

 Container VARCHAR(255) NULL,

 ContainerTag VARCHAR(255) NULL,

 FieldsToView VARCHAR(255) NULL,

 FilterSetting_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE UserSetting (

 primaryKey UUID NOT NULL,

 AppName VARCHAR(256) NULL,

 UserName VARCHAR(512) NULL,

 UserGuid UUID NULL,

 ModuleName VARCHAR(1024) NULL,

 ModuleGuid UUID NULL,

 SettName VARCHAR(256) NULL,

 SettGuid UUID NULL,

 SettLastAccessTime TIMESTAMP(3) NULL,

 StrVal VARCHAR(256) NULL,

 TxtVal TEXT NULL,

 IntVal INT NULL,

 BoolVal BOOLEAN NULL,

 GuidVal UUID NULL,

 DecimalVal DECIMAL(20,10) NULL,

 DateTimeVal TIMESTAMP(3) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ApplicationLog (

 primaryKey UUID NOT NULL,

 Category VARCHAR(64) NULL,

 EventId INT NULL,

 Priority INT NULL,

 Severity VARCHAR(32) NULL,

 Title VARCHAR(256) NULL,

 Timestamp TIMESTAMP(3) NULL,

 MachineName VARCHAR(32) NULL,

 AppDomainName VARCHAR(512) NULL,

 ProcessId VARCHAR(256) NULL,

 ProcessName VARCHAR(512) NULL,

 ThreadName VARCHAR(512) NULL,

 Win32ThreadId VARCHAR(128) NULL,

 Message VARCHAR(2500) NULL,

 FormattedMessage TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuObjType (

 primaryKey UUID NOT NULL,

 Name VARCHAR(255) NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuEntity (

 primaryKey UUID NOT NULL,

 ObjectPrimaryKey VARCHAR(38) NOT NULL,

 OperationTime TIMESTAMP(3) NOT NULL,

 OperationType VARCHAR(100) NOT NULL,

 ExecutionResult VARCHAR(12) NOT NULL,

 Source VARCHAR(255) NOT NULL,

 SerializedField TEXT NULL,

 User_m0 UUID NOT NULL,

 ObjectType_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAuField (

 primaryKey UUID NOT NULL,

 Field VARCHAR(100) NOT NULL,

 OldValue TEXT NULL,

 NewValue TEXT NULL,

 MainChange_m0 UUID NULL,

 AuditEntity_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAG (

 primaryKey UUID NOT NULL,

 Name VARCHAR(80) NOT NULL,

 Login VARCHAR(50) NULL,

 Pwd VARCHAR(50) NULL,

 IsUser BOOLEAN NOT NULL,

 IsGroup BOOLEAN NOT NULL,

 IsRole BOOLEAN NOT NULL,

 ConnString VARCHAR(255) NULL,

 Enabled BOOLEAN NULL,

 Email VARCHAR(80) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLG (

 primaryKey UUID NOT NULL,

 Group_m0 UUID NOT NULL,

 User_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMI (

 primaryKey UUID NOT NULL,

 User_m0 UUID NOT NULL,

 Agent_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Session (

 primaryKey UUID NOT NULL,

 UserKey UUID NULL,

 StartedAt TIMESTAMP(3) NULL,

 LastAccess TIMESTAMP(3) NULL,

 Closed BOOLEAN NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMS (

 primaryKey UUID NOT NULL,

 Name VARCHAR(100) NOT NULL,

 Type VARCHAR(100) NULL,

 IsAttribute BOOLEAN NOT NULL,

 IsOperation BOOLEAN NOT NULL,

 IsView BOOLEAN NOT NULL,

 IsClass BOOLEAN NOT NULL,

 SharedOper BOOLEAN NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMP (

 primaryKey UUID NOT NULL,

 Subject_m0 UUID NOT NULL,

 Agent_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMF (

 primaryKey UUID NOT NULL,

 FilterText TEXT NULL,

 Name VARCHAR(255) NULL,

 FilterTypeNView VARCHAR(255) NULL,

 Subject_m0 UUID NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMAC (

 primaryKey UUID NOT NULL,

 TypeAccess VARCHAR(7) NULL,

 Filter_m0 UUID NULL,

 Permition_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLO (

 primaryKey UUID NOT NULL,

 Class_m0 UUID NOT NULL,

 Operation_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLA (

 primaryKey UUID NOT NULL,

 View_m0 UUID NOT NULL,

 Attribute_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLV (

 primaryKey UUID NOT NULL,

 Class_m0 UUID NOT NULL,

 View_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE STORMLR (

 primaryKey UUID NOT NULL,

 StartDate TIMESTAMP(3) NULL,

 EndDate TIMESTAMP(3) NULL,

 Agent_m0 UUID NOT NULL,

 Role_m0 UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));




 ALTER TABLE SubscriptionStatisticsMonitor ADD CONSTRAINT FKd4374d44e65744f8b58148f4b5fa2332 FOREIGN KEY (Subscription) REFERENCES Subscription; 
CREATE INDEX Indexa55ebc15dd1947b082b099a038b24db2 on SubscriptionStatisticsMonitor (Subscription); 

 ALTER TABLE SubscriptionStatisticsMonitor ADD CONSTRAINT FK016be8ef844b463faf17c8eb3da75fd8 FOREIGN KEY (StatisticsMonitor) REFERENCES StatisticsMonitor; 
CREATE INDEX Index8f3857ad21dc4ce6be2481d9f697f1df on SubscriptionStatisticsMonitor (StatisticsMonitor); 

 ALTER TABLE StatisticsRecord ADD CONSTRAINT FK7079fee5c3e64d7c86325e7f9b1e1012 FOREIGN KEY (StatisticsSetting) REFERENCES StatisticsSetting; 
CREATE INDEX Indexbf939527cca846aabaa95176dbda2b17 on StatisticsRecord (StatisticsSetting); 

 ALTER TABLE StatisticsSetting ADD CONSTRAINT FKffb8d12b55c3485f9a9e5f141d553db1 FOREIGN KEY (Subscription) REFERENCES Subscription; 
CREATE INDEX Indexbad4b655694d440d934100402990c750 on StatisticsSetting (Subscription); 

 ALTER TABLE StatisticsCompressionSetting ADD CONSTRAINT FK4a665b3aa8fb4188b2ab951c57b9d51d FOREIGN KEY (StatisticsSetting) REFERENCES StatisticsSetting; 
CREATE INDEX Index52df4e5d6027476080a195c1ec179973 on StatisticsCompressionSetting (StatisticsSetting); 

 ALTER TABLE Message ADD CONSTRAINT FKa538fdb6d3894859a7693cfd6e72148c FOREIGN KEY (MessageType) REFERENCES MessageType; 
CREATE INDEX Index5045749503ab4da6a2adc838120f11f3 on Message (MessageType); 

 ALTER TABLE Message ADD CONSTRAINT FK59dec5cdcdb941aa8e018aebce8619d6 FOREIGN KEY (Recipient) REFERENCES Client; 
CREATE INDEX Index218471e3a95d4ea08471caf517692499 on Message (Recipient); 

 ALTER TABLE SendingPermission ADD CONSTRAINT FK02da36c6d9534bde91a698bfbaa7b055 FOREIGN KEY (MessageType) REFERENCES MessageType; 
CREATE INDEX Index98bd33160d364930bac5ad30601fe5d6 on SendingPermission (MessageType); 

 ALTER TABLE SendingPermission ADD CONSTRAINT FK9edc789d53a04fd48d84368bac65e885 FOREIGN KEY (Client) REFERENCES Client; 
CREATE INDEX Index0b830256aa4c4d2bb8bd8d6533b6e0cb on SendingPermission (Client); 

 ALTER TABLE Subscription ADD CONSTRAINT FK00831bba4ec94c4881e50ce0bc67e70d FOREIGN KEY (MessageType) REFERENCES MessageType; 
CREATE INDEX Index293400ab4910480daba5f0a78b873b87 on Subscription (MessageType); 

 ALTER TABLE Subscription ADD CONSTRAINT FK0caeecdd5d24425a941a372f58a1e521 FOREIGN KEY (Client) REFERENCES Client; 
CREATE INDEX Index37a2d86d45c24b328f9bf95fa8454e4a on Subscription (Client); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FKaeb0ff228056437fa5002b4e429bc05d FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FKfb1ba536529c4bca80cf14bf92410d7a FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FK89ac92cc6df44f0d9e9758ad29e0800a FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FKe59895b9d59f431aaedcc5e799e0e5ae FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK3aef9d933ac54e5487f1cd10b9dfb096 FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FKa2077ff3d2ab400c880321c19eb02efe FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK091280e60ab944a9b2e3363d4c124850 FOREIGN KEY (Group_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK3cd5a41855b94fb2b70a466c99c829ea FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FK679b1cb8d99b4676a65f066578151712 FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FK155765f56af44871ae94b8950a3dd897 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMP ADD CONSTRAINT FK3b479ec75bec4fd7913c04feee1bc9a7 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMP ADD CONSTRAINT FK32768c55824240c3a8a3e28eb3f53ff2 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMF ADD CONSTRAINT FK48a07ea23aa948be9e5a4b44e3afeb61 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMAC ADD CONSTRAINT FK6d84557bbb8b4853a55fd6965f685429 FOREIGN KEY (Filter_m0) REFERENCES STORMF; 

 ALTER TABLE STORMAC ADD CONSTRAINT FKd1d723d45afb4f2c8dee7f5477494cc1 FOREIGN KEY (Permition_m0) REFERENCES STORMP; 

 ALTER TABLE STORMLO ADD CONSTRAINT FKf947a1f83b664c328436312cd7dac7ae FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLO ADD CONSTRAINT FKae2bd90e6b2e4dd3bcb8a600763f7368 FOREIGN KEY (Operation_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FK3c0d199053b640d99ddb617a01a55783 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FK59eb5f11b3534f84931e3798756f5b71 FOREIGN KEY (Attribute_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FK3ff49480c4994f7cbfae46108b6dc9be FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FK0ad42f60f8d6486d9fa49d15f8547979 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK79a65a2b3f7740c4b97464bde738ca90 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK564871da808941749713db44c4d7248d FOREIGN KEY (Role_m0) REFERENCES STORMAG; 



 INSERT INTO STORMAG(primaryKey, Name, Login, Pwd, IsUser, IsGroup, IsRole, Enabled)
	VALUES (uuid_in(md5(random()::text)::cstring), 'Administrator', 'admin', 'D033E22AE348AEB5660FC2140AEC35850C4DA997', TRUE, FALSE, FALSE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'admin', null, null, FALSE, FALSE, TRUE, TRUE);

INSERT INTO STORMLR(primaryKey, Agent_m0, Role_m0)
	VALUES (uuid_in(md5(random()::text)::cstring), (SELECT primaryKey FROM STORMAG WHERE Name = 'Administrator'), (SELECT primaryKey FROM STORMAG WHERE Name = 'admin'));

INSERT INTO STORMS(primaryKey, Name, IsAttribute, IsOperation, IsView, IsClass, SharedOper)
	VALUES (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.SendingPermission', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.MessageType', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Subscription', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsCompressionSetting', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsSetting', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.SubscriptionStatisticsMonitor', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Client', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Message', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsMonitor', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.StatisticsRecord', FALSE, FALSE, FALSE, TRUE, TRUE)
	, (uuid_in(md5(random()::text)::cstring), 'NewPlatform.Flexberry.ServiceBus.Bus', FALSE, FALSE, FALSE, TRUE, TRUE);

INSERT INTO STORMP(primaryKey, Subject_m0, Agent_m0)
	SELECT uuid_in(md5(random()::text)::cstring), primaryKey, (SELECT primaryKey FROM STORMAG WHERE Name = 'admin') FROM STORMS;

INSERT INTO STORMAC(primaryKey, TypeAccess, Permition_m0)
	SELECT uuid_in(md5(random()::text)::cstring), 'Full', primaryKey FROM STORMP;
