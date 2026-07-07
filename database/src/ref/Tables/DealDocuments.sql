-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[DealDocuments] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [DealDetailDealId] nvarchar(64) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_DealDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DealDocuments_DealDetails_DealDetailDealId] FOREIGN KEY ([DealDetailDealId]) REFERENCES [ref].[DealDetails] ([DealId]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_DealDocuments]));
GO
CREATE INDEX [IX_DealDocuments_DealDetailDealId] ON [ref].[DealDocuments] ([DealDetailDealId]);
