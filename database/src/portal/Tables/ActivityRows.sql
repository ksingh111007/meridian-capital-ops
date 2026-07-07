-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[ActivityRows] (
    [Id] bigint NOT NULL IDENTITY,
    [InvestorId] nvarchar(64) NOT NULL,
    [Date] date NOT NULL,
    [Fund] nvarchar(400) NOT NULL,
    [Type] nvarchar(400) NOT NULL,
    [Reference] nvarchar(400) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_ActivityRows] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ActivityRows_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_ActivityRows]));
GO
CREATE INDEX [IX_ActivityRows_InvestorId_Date] ON [portal].[ActivityRows] ([InvestorId], [Date]);
