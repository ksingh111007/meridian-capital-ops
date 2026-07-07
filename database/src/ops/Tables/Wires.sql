-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[Wires] (
    [Id] nvarchar(64) NOT NULL,
    [Ref] nvarchar(400) NOT NULL,
    [Direction] nvarchar(400) NOT NULL,
    [Counterparty] nvarchar(400) NOT NULL,
    [Type] nvarchar(400) NOT NULL,
    [LinkedRef] nvarchar(400) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Time] nvarchar(400) NOT NULL,
    [Date] date NOT NULL,
    [Rail] nvarchar(400) NOT NULL,
    [Status] nvarchar(400) NOT NULL,
    [ExceptionReason] nvarchar(400) NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_Wires] PRIMARY KEY ([Id])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_Wires]));
GO
CREATE INDEX [IX_Wires_Status] ON [ops].[Wires] ([Status]);
