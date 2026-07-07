-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ref].[InvestorProfiles] (
    [InvestorId] nvarchar(64) NOT NULL,
    [Bank] nvarchar(400) NOT NULL,
    [AbaMasked] nvarchar(400) NOT NULL,
    [AccountMasked] nvarchar(400) NOT NULL,
    [BankingVerified] nvarchar(400) NOT NULL,
    [KycDocs] nvarchar(400) NOT NULL,
    [KycReviewDue] nvarchar(400) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_InvestorProfiles] PRIMARY KEY ([InvestorId]),
    CONSTRAINT [FK_InvestorProfiles_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ref_InvestorProfiles]));
