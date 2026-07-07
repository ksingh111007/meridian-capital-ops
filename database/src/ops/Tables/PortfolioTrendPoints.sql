-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[PortfolioTrendPoints] (
    [Id] bigint NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [Value] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [PortfolioSnapshotId] nvarchar(64) NOT NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_PortfolioTrendPoints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PortfolioTrendPoints_PortfolioSnapshots_PortfolioSnapshotId] FOREIGN KEY ([PortfolioSnapshotId]) REFERENCES [ops].[PortfolioSnapshots] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_PortfolioTrendPoints]));
GO
CREATE INDEX [IX_PortfolioTrendPoints_PortfolioSnapshotId] ON [ops].[PortfolioTrendPoints] ([PortfolioSnapshotId]);
