-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[ReconItems] (
    [Id] nvarchar(64) NOT NULL,
    [Date] date NOT NULL,
    [Description] nvarchar(400) NOT NULL,
    [Source] nvarchar(400) NOT NULL,
    [Book] decimal(18,2) NULL,
    [Custodian] decimal(18,2) NULL,
    [Diff] decimal(18,2) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [Assignee] nvarchar(400) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_ReconItems] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_ReconItems]));
GO
CREATE INDEX [IX_ReconItems_Status] ON [ops].[ReconItems] ([Status]);
