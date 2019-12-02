ALTER TABLE [Подписка] ADD [RestrictQueueLength] BIT NOT NULL DEFAULT 0;
ALTER TABLE [Подписка] ADD [MaxQueueLength] INT NOT NULL DEFAULT 1000;