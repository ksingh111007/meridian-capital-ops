-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CashForecastWeeks] (
    [Id] bigint NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [Label] nvarchar(400) NOT NULL,
    [Inflows] decimal(18,2) NOT NULL,
    [Outflows] decimal(18,2) NOT NULL,
    [Net] decimal(18,2) NOT NULL,
    [ProjectedBalance] decimal(18,2) NOT NULL,
    [CashPositionSnapshotId] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CashForecastWeeks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CashForecastWeeks_CashPositionSnapshots_CashPositionSnapshotId] FOREIGN KEY ([CashPositionSnapshotId]) REFERENCES [ops].[CashPositionSnapshots] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CashForecastWeeks]));
GO
CREATE INDEX [IX_CashForecastWeeks_CashPositionSnapshotId] ON [ops].[CashForecastWeeks] ([CashPositionSnapshotId]);
