-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [admin].[Roles])
INSERT INTO [admin].[Roles] ([Name], [Capabilities])
VALUES
    (N'Ops Analyst', N'{"Blotter":2,"Approvals":3,"Wires":2,"Recon":2,"RefData":1,"Admin":0}'),
    (N'Deal Lead', N'{"Blotter":2,"Approvals":3,"Wires":1,"Recon":0,"RefData":1,"Admin":0}'),
    (N'CIO', N'{"Blotter":1,"Approvals":3,"Wires":1,"Recon":1,"RefData":1,"Admin":0}'),
    (N'Counsel', N'{"Blotter":1,"Approvals":3,"Wires":0,"Recon":0,"RefData":1,"Admin":0}'),
    (N'Fund Accountant', N'{"Blotter":1,"Approvals":3,"Wires":2,"Recon":2,"RefData":2,"Admin":0}'),
    (N'Compliance', N'{"Blotter":1,"Approvals":3,"Wires":1,"Recon":1,"RefData":1,"Admin":1}'),
    (N'Ops Manager', N'{"Blotter":2,"Approvals":3,"Wires":2,"Recon":2,"RefData":2,"Admin":1}'),
    (N'Administrator', N'{"Blotter":4,"Approvals":4,"Wires":4,"Recon":4,"RefData":4,"Admin":4}');

IF NOT EXISTS (SELECT 1 FROM [admin].[StaffUsers])
INSERT INTO [admin].[StaffUsers] ([Id], [Name], [Initials], [Email], [RoleName], [FundAccess], [LastActive], [Status])
VALUES
    (N'u-jchen', N'Jordan Chen', N'JC', N'j.chen@meridiancredit.com', N'Ops Analyst', N'All funds', N'2m ago', N'Active'),
    (N'u-mreyes', N'Maria Reyes', N'MR', N'm.reyes@meridiancredit.com', N'Deal Lead', N'Fund III', N'1h ago', N'Active'),
    (N'u-spatel', N'Sanjay Patel', N'SP', N's.patel@meridiancredit.com', N'CIO', N'All funds', N'Today', N'Active'),
    (N'u-jokafor', N'J. Okafor', N'JO', N'j.okafor@meridiancredit.com', N'Counsel', N'All funds', N'Yesterday', N'Active'),
    (N'u-dwhitfield', N'Dana Whitfield', N'DW', N'd.whitfield@meridiancredit.com', N'Fund Accountant', N'Fund II, III', N'3h ago', N'Active'),
    (N'u-pnair', N'Priya Nair', N'PN', N'p.nair@meridiancredit.com', N'Compliance', N'All funds', N'Jul 03', N'Active'),
    (N'u-talvarez', N'Tom Alvarez', N'TA', N't.alvarez@meridiancredit.com', N'Ops Manager', N'All funds', N'5m ago', N'Active'),
    (N'u-akim', N'alex.kim@meridiancredit.com', N'AK', N'alex.kim@meridiancredit.com', N'Ops Analyst', N'Fund III', N'invited Jul 04', N'Invited'),
    (N'u-admin', N'Avery Whitman', N'AW', N'avery.whitman@meridiancredit.com', N'Administrator', N'All funds', N'—', N'Active');

IF NOT EXISTS (SELECT 1 FROM [admin].[Integrations])
INSERT INTO [admin].[Integrations] ([Name], [Type], [Direction], [LastSync], [Status], [Warning])
VALUES
    (N'Northern Trust', N'Custodian', N'Inbound', N'09:40', N'Connected', NULL),
    (N'State Street', N'Custodian', N'Inbound', N'09:38', N'Connected', NULL),
    (N'Silverpoint Bank', N'Bank / Facility', N'Two-way', N'09:40', N'Connected', NULL),
    (N'Investran', N'GL / Accounting', N'Two-way', N'09:15', N'Connected', NULL),
    (N'SOFR Rate Feed', N'Market Data', N'Inbound', N'09:00', N'Connected', NULL),
    (N'SWIFT Gateway', N'Payments', N'Outbound', N'09:31', N'Warning', N'Signing certificate expires Jul 13. Rotate credentials to avoid outbound wire disruption.');

IF NOT EXISTS (SELECT 1 FROM [admin].[NotificationRules])
INSERT INTO [admin].[NotificationRules] ([Id], [Name], [Trigger], [Channel], [Recipients], [Enabled])
VALUES
    (N'nr-1', N'Call due soon', N'Capital call due in 3 days', N'Email', N'Ops + Deal Lead', 1),
    (N'nr-2', N'Wire exception', N'Wire failed / rejected', N'Slack + Email', N'#ops-alerts, Treasury', 1),
    (N'nr-3', N'Approval overdue', N'Pending approval > SLA', N'Email', N'Approver + Manager', 1),
    (N'nr-4', N'Recon break', N'Break > $100k', N'Email', N'Controller', 1),
    (N'nr-5', N'Distribution posted', N'Distribution finalised', N'Email', N'Investor Relations', 1),
    (N'nr-6', N'New commitment', N'LP commitment added', N'Email', N'Compliance (KYC)', 1),
    (N'nr-7', N'Facility utilisation', N'Utilisation > 80%', N'Email', N'Treasury', 0);

IF NOT EXISTS (SELECT 1 FROM [admin].[NotificationChannels])
INSERT INTO [admin].[NotificationChannels] ([Name], [Detail], [Connected])
VALUES
    (N'Email', N'connected', 1),
    (N'Slack', N'#ops-alerts', 1),
    (N'SMS', N'Twilio', 1),
    (N'Webhook', N'add', 0);

