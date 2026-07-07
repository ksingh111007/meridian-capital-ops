-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[DistributionTiers] (
    [Id] bigint NOT NULL IDENTITY,
    [Tier] nvarchar(400) NOT NULL,
    [Basis] nvarchar(400) NOT NULL,
    [Rate] nvarchar(400) NOT NULL,
    [Distributed] decimal(18,2) NOT NULL,
    [LpShare] decimal(18,2) NULL,
    [GpShare] decimal(18,2) NULL,
    [PoolLeft] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [DistributionId] nvarchar(64) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_DistributionTiers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DistributionTiers_Distributions_DistributionId] FOREIGN KEY ([DistributionId]) REFERENCES [ops].[Distributions] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_DistributionTiers]));
GO
CREATE INDEX [IX_DistributionTiers_DistributionId] ON [ops].[DistributionTiers] ([DistributionId]);
