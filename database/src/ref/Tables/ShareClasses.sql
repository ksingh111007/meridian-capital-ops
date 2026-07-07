-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[ShareClasses] (
    [Id] bigint NOT NULL IDENTITY,
    [FundId] nvarchar(64) NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [MgmtFeePct] decimal(18,2) NOT NULL,
    [CarryPct] decimal(18,2) NOT NULL,
    [PrefPct] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_ShareClasses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShareClasses_Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [ref].[Funds] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_ShareClasses]));
GO
CREATE INDEX [IX_ShareClasses_FundId] ON [ref].[ShareClasses] ([FundId]);
