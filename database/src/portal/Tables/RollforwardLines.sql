-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[RollforwardLines] (
    [Id] bigint NOT NULL IDENTITY,
    [InvestorId] nvarchar(64) NOT NULL,
    [Period] nvarchar(400) NOT NULL,
    [SortOrder] int NOT NULL,
    [Label] nvarchar(400) NOT NULL,
    [Kind] nvarchar(400) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_RollforwardLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RollforwardLines_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_RollforwardLines]));
GO
CREATE INDEX [IX_RollforwardLines_InvestorId] ON [portal].[RollforwardLines] ([InvestorId]);
