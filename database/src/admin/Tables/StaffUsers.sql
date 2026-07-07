-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [admin].[StaffUsers] (
    [Id] nvarchar(64) NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [Initials] nvarchar(400) NOT NULL,
    [Email] nvarchar(400) NOT NULL,
    [RoleName] nvarchar(400) NOT NULL,
    [FundAccess] nvarchar(400) NOT NULL,
    [LastActive] nvarchar(400) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_StaffUsers] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[admin_StaffUsers]));
