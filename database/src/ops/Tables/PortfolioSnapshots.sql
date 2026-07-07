-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[PortfolioSnapshots] (
    [Id] nvarchar(64) NOT NULL,
    [AsOf] date NOT NULL,
    [InvestedCapital] decimal(18,2) NOT NULL,
    [ActiveDeals] int NOT NULL,
    [NetIrrPct] decimal(18,2) NOT NULL,
    [BlendedMoic] decimal(18,2) NOT NULL,
    [OnWatchCount] int NOT NULL,
    [OnWatchExposure] decimal(18,2) NOT NULL,
    [PerformingPct] decimal(18,2) NOT NULL,
    [WatchPct] decimal(18,2) NOT NULL,
    [NonAccrualPct] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_PortfolioSnapshots] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_PortfolioSnapshots]));
