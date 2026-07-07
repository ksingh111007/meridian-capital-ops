-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[CallAllocations] (
    [Id] bigint NOT NULL IDENTITY,
    [InvestorId] nvarchar(64) NOT NULL,
    [InvestorName] nvarchar(400) NOT NULL,
    [Commitment] decimal(18,2) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [WireStatus] nvarchar(20) NOT NULL,
    [CapitalCallId] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_CallAllocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CallAllocations_CapitalCalls_CapitalCallId] FOREIGN KEY ([CapitalCallId]) REFERENCES [ops].[CapitalCalls] ([Id]) ON DELETE CASCADE
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_CallAllocations]));
GO
CREATE INDEX [IX_CallAllocations_CapitalCallId] ON [ops].[CallAllocations] ([CapitalCallId]);
