-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [ops].[WorkflowStages])
INSERT INTO [ops].[WorkflowStages] ([Order], [Name], [ApproverRole], [SlaDays], [AutoAdvance], [Required], [Terminal])
VALUES
    (1, N'Operations', N'Ops Analyst', 1, 0, 1, 0),
    (2, N'Front Office', N'Deal Lead', 1, 0, 1, 0),
    (3, N'CIO', N'CIO', 2, 0, 1, 0),
    (4, N'Legal', N'Counsel', 2, 0, 1, 0),
    (5, N'Ops Final Review', N'Ops Manager', 1, 0, 1, 0),
    (6, N'Accounting', N'Fund Accountant', 1, 0, 1, 0),
    (7, N'Book', N'System', NULL, 1, 1, 0),
    (8, N'Custodians Notified', N'System', NULL, 1, 0, 0),
    (9, N'Completed', N'', NULL, 0, 0, 1);

IF NOT EXISTS (SELECT 1 FROM [ops].[EscalationRules])
INSERT INTO [ops].[EscalationRules] ([Id], [Kind], [Condition], [Effect], [Enabled], [ThresholdAmount], [RequiredRoles])
VALUES
    (N'esc-amount', N'AmountThreshold', N'Call amount > $20M', N'require CIO + Compliance sign-off', 1, 20, N'["CIO","Compliance"]'),
    (N'esc-crossfund', N'CrossFundAllocation', N'Cross-fund allocation', N'require Legal review', 1, NULL, N'["Counsel"]'),
    (N'esc-newbank', N'NewBankAccount', N'Wire to a new bank account', N'require dual authorization', 1, NULL, N'[]');

