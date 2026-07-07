-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CapitalCalls] (
    [Id] nvarchar(64) NOT NULL,
    [Ref] nvarchar(400) NOT NULL,
    [DealId] nvarchar(64) NOT NULL,
    [DealName] nvarchar(400) NOT NULL,
    [FundId] nvarchar(64) NOT NULL,
    [Tranche] nvarchar(400) NOT NULL,
    [Borrower] nvarchar(400) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [DueDate] date NOT NULL,
    [Basis] nvarchar(20) NOT NULL,
    [CurrentStage] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [PendingEscalations] nvarchar(2000) NOT NULL,
    [EscalationGateStage] int NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CapitalCalls] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CapitalCalls]));
