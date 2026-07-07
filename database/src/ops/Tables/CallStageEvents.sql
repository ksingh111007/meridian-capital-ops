-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CallStageEvents] (
    [Id] bigint NOT NULL IDENTITY,
    [Stage] int NOT NULL,
    [State] nvarchar(20) NOT NULL,
    [Actor] nvarchar(400) NULL,
    [Date] date NULL,
    [Note] nvarchar(400) NULL,
    [Comment] nvarchar(2000) NULL,
    [CapitalCallId] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CallStageEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CallStageEvents_CapitalCalls_CapitalCallId] FOREIGN KEY ([CapitalCallId]) REFERENCES [ops].[CapitalCalls] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CallStageEvents]));
GO
CREATE INDEX [IX_CallStageEvents_CapitalCallId] ON [ops].[CallStageEvents] ([CapitalCallId]);
