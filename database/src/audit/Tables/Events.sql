-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [audit].[Events] (
    [Id] bigint NOT NULL IDENTITY,
    [At] datetime2 NOT NULL,
    [Actor] nvarchar(400) NOT NULL,
    [Action] nvarchar(400) NOT NULL,
    [Tone] nvarchar(400) NOT NULL,
    [Subject] nvarchar(400) NOT NULL,
    [Detail] nvarchar(2000) NOT NULL,
    [Seal] nvarchar(12) NOT NULL,
    [PreviousSeal] nvarchar(12) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Events] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[audit_Events]));
GO
CREATE UNIQUE INDEX [IX_Events_PreviousSeal] ON [audit].[Events] ([PreviousSeal]) WHERE [PreviousSeal] IS NOT NULL;
