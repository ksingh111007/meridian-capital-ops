-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[CurrencyRates] (
    [Code] nvarchar(3) NOT NULL,
    [Rate] decimal(18,6) NOT NULL,
    [Note] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CurrencyRates] PRIMARY KEY ([Code])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_CurrencyRates]));
