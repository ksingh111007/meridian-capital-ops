-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[IrConfig] (
    [Id] nvarchar(64) NOT NULL,
    [ManagerName] nvarchar(400) NOT NULL,
    [ManagerInitials] nvarchar(400) NOT NULL,
    [ManagerTitle] nvarchar(400) NOT NULL,
    [Email] nvarchar(400) NOT NULL,
    [Phone] nvarchar(400) NOT NULL,
    [Hours] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_IrConfig] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_IrConfig]));
