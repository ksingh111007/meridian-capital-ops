-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[EscalationRules] (
    [Id] nvarchar(64) NOT NULL,
    [Kind] nvarchar(40) NOT NULL,
    [Condition] nvarchar(400) NOT NULL,
    [Effect] nvarchar(400) NOT NULL,
    [Enabled] bit NOT NULL,
    [ThresholdAmount] decimal(18,2) NULL,
    [RequiredRoles] nvarchar(2000) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_EscalationRules] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_EscalationRules]));
