-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CashForecastBars] (
    [Id] bigint NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [Height] int NOT NULL,
    [CashPositionSnapshotId] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CashForecastBars] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CashForecastBars_CashPositionSnapshots_CashPositionSnapshotId] FOREIGN KEY ([CashPositionSnapshotId]) REFERENCES [ops].[CashPositionSnapshots] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CashForecastBars]));
GO
CREATE INDEX [IX_CashForecastBars_CashPositionSnapshotId] ON [ops].[CashForecastBars] ([CashPositionSnapshotId]);
