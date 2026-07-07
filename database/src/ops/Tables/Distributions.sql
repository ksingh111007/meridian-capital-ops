-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[Distributions] (
    [Id] nvarchar(64) NOT NULL,
    [Ref] nvarchar(400) NOT NULL,
    [FundId] nvarchar(64) NOT NULL,
    [Distributable] decimal(18,2) NOT NULL,
    [LpTotal] decimal(18,2) NOT NULL,
    [GpTotal] decimal(18,2) NOT NULL,
    [PaymentDate] date NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [WaterfallType] nvarchar(20) NOT NULL,
    [SourceNote] nvarchar(400) NOT NULL,
    [Recallable] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Distributions] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_Distributions]));
