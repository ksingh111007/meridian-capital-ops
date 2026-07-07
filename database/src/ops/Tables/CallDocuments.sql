-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CallDocuments] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(400) NOT NULL,
    [By] nvarchar(400) NOT NULL,
    [Date] date NOT NULL,
    [CapitalCallId] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CallDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CallDocuments_CapitalCalls_CapitalCallId] FOREIGN KEY ([CapitalCallId]) REFERENCES [ops].[CapitalCalls] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CallDocuments]));
GO
CREATE INDEX [IX_CallDocuments_CapitalCallId] ON [ops].[CallDocuments] ([CapitalCallId]);
