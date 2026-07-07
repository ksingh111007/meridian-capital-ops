-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[Deals] (
    [Id] nvarchar(64) NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [Borrower] nvarchar(400) NOT NULL,
    [Sector] nvarchar(400) NOT NULL,
    [Country] nvarchar(400) NOT NULL,
    [FundId] nvarchar(64) NOT NULL,
    [Tranche] nvarchar(400) NOT NULL,
    [Invested] decimal(18,2) NOT NULL,
    [Outstanding] decimal(18,2) NOT NULL,
    [Spread] nvarchar(400) NOT NULL,
    [NetIrrPct] decimal(18,2) NOT NULL,
    [IrrTrend] nvarchar(400) NOT NULL,
    [Moic] decimal(18,2) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Deals] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_Deals]));
