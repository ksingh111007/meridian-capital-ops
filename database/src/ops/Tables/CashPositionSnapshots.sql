-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CashPositionSnapshots] (
    [Id] nvarchar(64) NOT NULL,
    [AsOf] date NOT NULL,
    [FundId] nvarchar(64) NOT NULL,
    [CashOnHand] decimal(18,2) NOT NULL,
    [AccountsCount] int NOT NULL,
    [UncalledCapital] decimal(18,2) NOT NULL,
    [UncalledLps] int NOT NULL,
    [FacilityHeadroom] decimal(18,2) NOT NULL,
    [FacilityLimit] decimal(18,2) NOT NULL,
    [Net30DayProjection] decimal(18,2) NOT NULL,
    [CoverageRatio] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CashPositionSnapshots] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CashPositionSnapshots]));
