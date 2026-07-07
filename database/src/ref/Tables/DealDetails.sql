-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[DealDetails] (
    [DealId] nvarchar(64) NOT NULL,
    [FairValue] decimal(18,2) NOT NULL,
    [Facility] decimal(18,2) NOT NULL,
    [Drawn] decimal(18,2) NOT NULL,
    [Maturity] nvarchar(400) NOT NULL,
    [SpreadFloor] nvarchar(400) NOT NULL,
    [UpfrontFeePct] decimal(18,2) NOT NULL,
    [InternalRating] nvarchar(400) NOT NULL,
    [RiskTrend] nvarchar(400) NOT NULL,
    [Covenants] nvarchar(400) NOT NULL,
    [NetLeverage] nvarchar(400) NOT NULL,
    [LastReview] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_DealDetails] PRIMARY KEY ([DealId]),
    CONSTRAINT [FK_DealDetails_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [ref].[Deals] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_DealDetails]));
