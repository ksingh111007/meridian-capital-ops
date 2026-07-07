-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[Drawdowns] (
    [Id] nvarchar(64) NOT NULL,
    [Facility] nvarchar(400) NOT NULL,
    [Lender] nvarchar(400) NOT NULL,
    [Purpose] nvarchar(400) NOT NULL,
    [DealId] nvarchar(64) NULL,
    [LinkedCallId] nvarchar(64) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Rate] nvarchar(400) NOT NULL,
    [DrawDate] date NOT NULL,
    [RepayBy] date NULL,
    [Status] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Drawdowns] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_Drawdowns]));
