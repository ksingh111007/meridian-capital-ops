-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[DistributionPayouts] (
    [Id] bigint NOT NULL IDENTITY,
    [InvestorId] nvarchar(64) NOT NULL,
    [InvestorName] nvarchar(400) NOT NULL,
    [Commitment] decimal(18,2) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PctOfLpTotal] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [BlockedReason] nvarchar(400) NULL,
    [WireRef] nvarchar(400) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [DistributionId] nvarchar(64) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_DistributionPayouts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DistributionPayouts_Distributions_DistributionId] FOREIGN KEY ([DistributionId]) REFERENCES [ops].[Distributions] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_DistributionPayouts]));
GO
CREATE INDEX [IX_DistributionPayouts_DistributionId] ON [ops].[DistributionPayouts] ([DistributionId]);
