-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[KpiSnapshots] (
    [Id] bigint NOT NULL IDENTITY,
    [ScreenKey] nvarchar(60) NOT NULL,
    [MetricKey] nvarchar(60) NOT NULL,
    [NumericValue] decimal(18,4) NULL,
    [TextValue] nvarchar(400) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_KpiSnapshots] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_KpiSnapshots]));
GO
CREATE UNIQUE INDEX [IX_KpiSnapshots_ScreenKey_MetricKey] ON [ops].[KpiSnapshots] ([ScreenKey], [MetricKey]);
