-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[FundPositions] (
    [Id] bigint NOT NULL IDENTITY,
    [InvestorId] nvarchar(64) NOT NULL,
    [FundId] nvarchar(64) NOT NULL,
    [FundName] nvarchar(400) NOT NULL,
    [Vintage] int NOT NULL,
    [Commitment] decimal(18,2) NOT NULL,
    [PaidIn] decimal(18,2) NOT NULL,
    [Distributions] decimal(18,2) NOT NULL,
    [Nav] decimal(18,2) NOT NULL,
    [NetIrrPct] decimal(18,2) NOT NULL,
    [Tvpi] decimal(18,2) NOT NULL,
    [Dpi] decimal(18,2) NOT NULL,
    [CalledPct] decimal(18,2) NOT NULL,
    [CalledAmount] decimal(18,2) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_FundPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FundPositions_Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [ref].[Funds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_FundPositions_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_FundPositions]));
GO
CREATE INDEX [IX_FundPositions_FundId] ON [portal].[FundPositions] ([FundId]);
GO
CREATE UNIQUE INDEX [IX_FundPositions_InvestorId_FundId] ON [portal].[FundPositions] ([InvestorId], [FundId]);
