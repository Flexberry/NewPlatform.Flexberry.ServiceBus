



CREATE TABLE SubStatisticsMonitor (

 primaryKey UUID NOT NULL,

 Код INT NOT NULL,

 Категория VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Подписка UUID NOT NULL,

 StatisticsMonitor UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Шина (

 primaryKey UUID NOT NULL,

 InteropАдрес VARCHAR(255) NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Адрес VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 Description TEXT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatRecord (

 primaryKey UUID NOT NULL,

 Since TIMESTAMP(3) NOT NULL,

 "To" TIMESTAMP(3) NOT NULL,

 StatInterval VARCHAR(12) NOT NULL,

 SentCount INT NULL,

 ReceivedCount INT NULL,

 ErrorsCount INT NULL,

 UniqueErrorsCount INT NULL,

 ConnectionCount INT NULL,

 QueueLength INT NULL,

 AvgTimeSent INT NULL,

 AvgTimeSql INT NULL,

 StatSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatSetting (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 Подписка UUID NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Клиент (

 primaryKey UUID NOT NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Адрес VARCHAR(255) NULL,

 DnsIdentity VARCHAR(255) NULL,

 Description TEXT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE CompressionSetting (

 primaryKey UUID NOT NULL,

 TargetCompression VARCHAR(12) NOT NULL,

 LifetimeLimit INT NOT NULL,

 LifetimeUnits VARCHAR(6) NOT NULL,

 Period INT NOT NULL,

 PeriodUnits VARCHAR(6) NOT NULL,

 NextCompressionTime TIMESTAMP(3) NOT NULL,

 LastCompressionTime TIMESTAMP(3) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 StatSetting UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Сообщение (

 primaryKey UUID NOT NULL,

 ВремяСледующейОтправки TIMESTAMP(3) NOT NULL,

 ВремяФормирования TIMESTAMP(3) NOT NULL,

 Отправляется BOOLEAN NULL,

 FailsCount INT NULL,

 Отправитель VARCHAR(255) NULL,

 Тело TEXT NULL,

 ВложениеДляБазы TEXT NULL,

 Приоритет INT NULL,

 ИмяГруппы VARCHAR(255) NULL,

 Тэги VARCHAR NULL,

 LogMessages VARCHAR NULL,

 ТипСообщения_m0 UUID NOT NULL,

 Получатель_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE OutboundMessageTypeRestriction (

 primaryKey UUID NOT NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 ТипСообщения UUID NOT NULL,

 Клиент UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE Подписка (

 primaryKey UUID NOT NULL,

 Описание TEXT NULL,

 ExpiryDate TIMESTAMP(3) NOT NULL,

 IsCallback BOOLEAN NULL,

 ПередаватьПо VARCHAR(4) NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 ТипСообщения_m0 UUID NOT NULL,

 Клиент_m0 UUID NOT NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE StatisticsMonitor (

 primaryKey UUID NOT NULL,

 Логин VARCHAR(255) NULL,

 Наименование VARCHAR(255) NOT NULL,

 ДоступенДругимПользователям BOOLEAN NULL,

 CreateTime TIMESTAMP(3) NULL,

 Creator VARCHAR(255) NULL,

 EditTime TIMESTAMP(3) NULL,

 Editor VARCHAR(255) NULL,

 PRIMARY KEY (primaryKey));


CREATE TABLE ТипСообщения (

 primaryKey UUID NOT NULL,

 Ид VARCHAR(255) NULL,

 Наименование VARCHAR(255) NULL,

 Комментарий TEXT NULL,

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




 ALTER TABLE SubStatisticsMonitor ADD CONSTRAINT FKcc611072523d46fc981668caaaf9d24e FOREIGN KEY (Подписка) REFERENCES Подписка; 
CREATE INDEX Index29296d1a70ce45ff9a45cc1bfe560386 on SubStatisticsMonitor (Подписка); 

 ALTER TABLE SubStatisticsMonitor ADD CONSTRAINT FK0662fb51bfa94a858e6711d5d6b155bb FOREIGN KEY (StatisticsMonitor) REFERENCES StatisticsMonitor; 
CREATE INDEX Index97e3aa0fbe7a4ee1b04aa4bcaccef8bc on SubStatisticsMonitor (StatisticsMonitor); 

 ALTER TABLE StatRecord ADD CONSTRAINT FKea3fa759a3e240408ac6aae69b99fa3d FOREIGN KEY (StatSetting) REFERENCES StatSetting; 
CREATE INDEX Index83bbb2df72224b8f8524769d498c165b on StatRecord (StatSetting); 

 ALTER TABLE StatSetting ADD CONSTRAINT FKe25a1ef5bd414c8c8ffa0fe44bcb04c6 FOREIGN KEY (Подписка) REFERENCES Подписка; 
CREATE INDEX Indexee30dd66b8884de9a3d496f389851a15 on StatSetting (Подписка); 

 ALTER TABLE CompressionSetting ADD CONSTRAINT FK926c45e9a28a4799b5d8b430c98f8645 FOREIGN KEY (StatSetting) REFERENCES StatSetting; 
CREATE INDEX Indexf5f713e0404142c39beacc9837169bf2 on CompressionSetting (StatSetting); 

 ALTER TABLE Сообщение ADD CONSTRAINT FKebbe94c7c8e54a73b933fe123afa88f5 FOREIGN KEY (ТипСообщения_m0) REFERENCES ТипСообщения; 
CREATE INDEX Indexf2423394e9874a1ca96defaa2f5814bc on Сообщение (ТипСообщения_m0); 

 ALTER TABLE Сообщение ADD CONSTRAINT FKd3fa817300974d3a9a535a8adc129cc1 FOREIGN KEY (Получатель_m0) REFERENCES Клиент; 
CREATE INDEX Indexd22df496f5724d29b99fca6382b4827c on Сообщение (Получатель_m0); 

 ALTER TABLE OutboundMessageTypeRestriction ADD CONSTRAINT FKa4f04f61273541a48d7b8c1a8ede29db FOREIGN KEY (ТипСообщения) REFERENCES ТипСообщения; 
CREATE INDEX Index72525846c6034993a2c9564efa0ea07b on OutboundMessageTypeRestriction (ТипСообщения); 

 ALTER TABLE OutboundMessageTypeRestriction ADD CONSTRAINT FK4409f1dd78cd44e89b22d34fbfdd6723 FOREIGN KEY (Клиент) REFERENCES Клиент; 
CREATE INDEX Index24ee695fe47340ceba9012bb4dafab74 on OutboundMessageTypeRestriction (Клиент); 

 ALTER TABLE Подписка ADD CONSTRAINT FK58953dc429724e9fbc47ba68e74af34d FOREIGN KEY (ТипСообщения_m0) REFERENCES ТипСообщения; 
CREATE INDEX Indexbe9f00359082490b9a4fbafb27e07b96 on Подписка (ТипСообщения_m0); 

 ALTER TABLE Подписка ADD CONSTRAINT FK54a87c383ff445e6860a1bb9f0a33e1c FOREIGN KEY (Клиент_m0) REFERENCES Клиент; 
CREATE INDEX Index4e12a17bc83b4d828a0e2f5e070c0d9e on Подписка (Клиент_m0); 

 ALTER TABLE STORMWEBSEARCH ADD CONSTRAINT FK1dbf2372f9b94c53a5ccdc31f68d5261 FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERDETAIL ADD CONSTRAINT FKe250f2729aed410a9919f483d78884ac FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMFILTERLOOKUP ADD CONSTRAINT FK6f9707b474dd4b8bbafd13801580307b FOREIGN KEY (FilterSetting_m0) REFERENCES STORMFILTERSETTING; 

 ALTER TABLE STORMAuEntity ADD CONSTRAINT FKe1dbf77e4d1d48a69c4d2bfba7ac4dcb FOREIGN KEY (ObjectType_m0) REFERENCES STORMAuObjType; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK494004879b274cfcbc5255661e7c5dfc FOREIGN KEY (MainChange_m0) REFERENCES STORMAuField; 

 ALTER TABLE STORMAuField ADD CONSTRAINT FK8d13457e9eec4c02b1256226689b12ec FOREIGN KEY (AuditEntity_m0) REFERENCES STORMAuEntity; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK12c5418ea03b4741a6d902cbcda5b876 FOREIGN KEY (Group_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLG ADD CONSTRAINT FK3eddb0994ae141bd8c8faab08cc54cda FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FK60b4bf69ae154cb9ac52d08680e349fa FOREIGN KEY (User_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMI ADD CONSTRAINT FKb876d46e78634cc093fc4305338a259d FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMP ADD CONSTRAINT FK3f8f356cf9914a1dafcaf1c0a4ce6997 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMP ADD CONSTRAINT FKe41e1decdbb04ea5909066e19a1e35ee FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMF ADD CONSTRAINT FKdd2ab701440f4c979e1c7c189efd9453 FOREIGN KEY (Subject_m0) REFERENCES STORMS; 

 ALTER TABLE STORMAC ADD CONSTRAINT FK57b29d389762469ab3bf08319b138461 FOREIGN KEY (Filter_m0) REFERENCES STORMF; 

 ALTER TABLE STORMAC ADD CONSTRAINT FKf32c9d03f74b41d99b4e864adad50cf5 FOREIGN KEY (Permition_m0) REFERENCES STORMP; 

 ALTER TABLE STORMLO ADD CONSTRAINT FK7bb4a11c6a1f40eebde3860846b236d0 FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLO ADD CONSTRAINT FKd8844a8019744bf9891681a010797127 FOREIGN KEY (Operation_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FKa09dc771bb794e718e7d7a8dbbd9c442 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLA ADD CONSTRAINT FKeef36131c3c94bd6a2e9a7007ae57afb FOREIGN KEY (Attribute_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FKfa13b0d54df34dd1a449786d406280a1 FOREIGN KEY (Class_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLV ADD CONSTRAINT FK50c5353fba05493ba01c82687d63c386 FOREIGN KEY (View_m0) REFERENCES STORMS; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK1ff60e31c7174e46a1c9bbb222c28db0 FOREIGN KEY (Agent_m0) REFERENCES STORMAG; 

 ALTER TABLE STORMLR ADD CONSTRAINT FK239358b09a5548bf8a530531dfd7d13b FOREIGN KEY (Role_m0) REFERENCES STORMAG; 

