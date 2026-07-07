-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [portal].[AccountSnapshots] (
    [InvestorId] nvarchar(64) NOT NULL,
    [AsOf] date NOT NULL,
    [Commitment] decimal(18,2) NOT NULL,
    [PaidIn] decimal(18,2) NOT NULL,
    [Distributions] decimal(18,2) NOT NULL,
    [Nav] decimal(18,2) NOT NULL,
    [NetIrrPct] decimal(18,2) NOT NULL,
    [Tvpi] decimal(18,2) NOT NULL,
    [NetInvested] decimal(18,2) NOT NULL,
    [NextCallDue] date NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_AccountSnapshots] PRIMARY KEY ([InvestorId]),
    CONSTRAINT [FK_AccountSnapshots_Investors_InvestorId] FOREIGN KEY ([InvestorId]) REFERENCES [ref].[Investors] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[portal_AccountSnapshots]));
