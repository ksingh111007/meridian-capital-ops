-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[RollforwardAmounts] (
    [Id] bigint NOT NULL IDENTITY,
    [FundId] nvarchar(64) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [PortalRollforwardLineId] bigint NOT NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_RollforwardAmounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RollforwardAmounts_RollforwardLines_PortalRollforwardLineId] FOREIGN KEY ([PortalRollforwardLineId]) REFERENCES [portal].[RollforwardLines] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_RollforwardAmounts]));
GO
CREATE INDEX [IX_RollforwardAmounts_PortalRollforwardLineId] ON [portal].[RollforwardAmounts] ([PortalRollforwardLineId]);
