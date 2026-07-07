-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[Contacts] (
    [Id] nvarchar(64) NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [Initials] nvarchar(400) NOT NULL,
    [InvestorId] nvarchar(64) NOT NULL,
    [InvestorName] nvarchar(400) NOT NULL,
    [Role] nvarchar(400) NOT NULL,
    [FundsVisible] nvarchar(400) NOT NULL,
    [Statements] nvarchar(400) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contacts_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_Contacts]));
GO
CREATE INDEX [IX_Contacts_InvestorId] ON [portal].[Contacts] ([InvestorId]);
