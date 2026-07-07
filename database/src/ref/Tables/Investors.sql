-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[Investors] (
    [Id] nvarchar(64) NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [Type] nvarchar(400) NOT NULL,
    [KycStatus] nvarchar(400) NOT NULL,
    [WireInstructionsOnFile] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Investors] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_Investors]));
