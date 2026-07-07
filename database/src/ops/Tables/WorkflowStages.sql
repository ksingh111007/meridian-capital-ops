-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.
CREATE TABLE [ops].[WorkflowStages] (
    [Order] int NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [ApproverRole] nvarchar(400) NOT NULL,
    [SlaDays] int NULL,
    [AutoAdvance] bit NOT NULL,
    [Required] bit NOT NULL,
    [Terminal] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] nvarchar(100) NOT NULL DEFAULT N'system',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ModifiedAtUtc] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo]),
    CONSTRAINT [PK_WorkflowStages] PRIMARY KEY ([Order])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[ops_WorkflowStages]));
