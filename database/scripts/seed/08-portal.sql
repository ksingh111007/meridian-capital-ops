-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [portal].[Contacts])
INSERT INTO [portal].[Contacts] ([Id], [Name], [Initials], [InvestorId], [InvestorName], [Role], [FundsVisible], [Statements], [Status])
VALUES
    (N'pc-1', N'Karen Doyle', N'KD', N'inv-redwood', N'Redwood Pension', N'Primary', N'II, III', N'full', N'Active'),
    (N'pc-2', N'Mark Feld', N'MF', N'inv-blueharbor', N'Blue Harbor Endowment', N'Primary', N'III', N'full', N'Active'),
    (N'pc-3', N'Lucia Romano', N'LR', N'inv-cascade', N'Cascade Family Office', N'Viewer', N'II, III', N'full', N'Active'),
    (N'pc-4', N't.wells@granite…', N'TW', N'inv-granite', N'Granite State Insurance', N'Tax-only', N'II', N'tax', N'Invited'),
    (N'pc-5', N'David Chen', N'DC', N'inv-summit', N'Summit Investments', N'Primary', N'III', N'full', N'Active'),
    (N'pc-6', N'R. Okafor', N'RO', N'inv-oakmont', N'Oakmont Trust', N'Viewer', N'II', N'none', N'Disabled');

IF NOT EXISTS (SELECT 1 FROM [portal].[Capabilities])
INSERT INTO [portal].[Capabilities] ([Label], [SortOrder], [Enabled])
VALUES
    (N'View capital account balances', 1, 1),
    (N'Download capital account statements', 2, 1),
    (N'View & download tax documents (K-1)', 3, 1),
    (N'View capital activity (calls & distributions)', 4, 1),
    (N'View unfunded commitment', 5, 1),
    (N'Submit wire confirmations', 6, 0),
    (N'Message the IR team', 7, 1);

IF NOT EXISTS (SELECT 1 FROM [portal].[DocumentTypes])
INSERT INTO [portal].[DocumentTypes] ([Label], [SortOrder], [Exposed])
VALUES
    (N'Capital Account Statements', 1, 1),
    (N'Capital Call Notices', 2, 1),
    (N'Distribution Notices', 3, 1),
    (N'Schedule K-1 / Tax', 4, 1),
    (N'Audited fund financials', 5, 1),
    (N'Side letters', 6, 0);

IF NOT EXISTS (SELECT 1 FROM [portal].[FundPositions])
INSERT INTO [portal].[FundPositions] ([InvestorId], [FundId], [FundName], [Vintage], [Commitment], [PaidIn], [Distributions], [Nav], [NetIrrPct], [Tvpi], [Dpi], [CalledPct], [CalledAmount])
VALUES
    (N'inv-redwood', N'fund-iii', N'Meridian Credit III', 2024, 40, 28.4, 10.8, 24.1, 14.1, 1.23, 0.38, 71, 28.4),
    (N'inv-redwood', N'fund-ii', N'Meridian Credit II', 2021, 18, 17.3, 7.8, 10.7, 11.8, 1.32, 0.62, 96, 17.3);

IF NOT EXISTS (SELECT 1 FROM [portal].[AccountSnapshots])
INSERT INTO [portal].[AccountSnapshots] ([InvestorId], [AsOf], [Commitment], [PaidIn], [Distributions], [Nav], [NetIrrPct], [Tvpi], [NetInvested], [NextCallDue])
VALUES
    (N'inv-redwood', N'2026-06-30', 58, 41.2, 18.6, 34.8, 13.2, 1.3, 22.6, N'2026-07-08');

IF NOT EXISTS (SELECT 1 FROM [portal].[RollforwardLines])
INSERT INTO [portal].[RollforwardLines] ([InvestorId], [Period], [SortOrder], [Label], [Kind], [Total])
VALUES
    (N'inv-redwood', N'Q2 2026', 1, N'Beginning NAV (Apr 1)', N'start', 34),
    (N'inv-redwood', N'Q2 2026', 2, N'Contributions', N'positive', 5),
    (N'inv-redwood', N'Q2 2026', 3, N'Distributions', N'negative', -5.5),
    (N'inv-redwood', N'Q2 2026', 4, N'Change in value', N'positive', 1.3),
    (N'inv-redwood', N'Q2 2026', 5, N'Ending NAV (Jun 30)', N'end', 34.8);

IF NOT EXISTS (SELECT 1 FROM [portal].[RollforwardAmounts])
INSERT INTO [portal].[RollforwardAmounts] ([FundId], [Amount], [PortalRollforwardLineId])
SELECT v.FundId, v.Amount, l.[Id]
FROM (VALUES
    (N'fund-iii', 21.6, 1),
    (N'fund-ii', 12.4, 1),
    (N'fund-iii', 5, 2),
    (N'fund-iii', -3.4, 3),
    (N'fund-ii', -2.1, 3),
    (N'fund-iii', 0.9, 4),
    (N'fund-ii', 0.4, 4),
    (N'fund-iii', 24.1, 5),
    (N'fund-ii', 10.7, 5)
) v (FundId, Amount, SortOrder)
JOIN [portal].[RollforwardLines] l ON l.[InvestorId] = N'inv-redwood' AND l.[SortOrder] = v.SortOrder;

IF NOT EXISTS (SELECT 1 FROM [portal].[ActivityRows])
INSERT INTO [portal].[ActivityRows] ([InvestorId], [Date], [Fund], [Type], [Reference], [Amount], [Status])
VALUES
    (N'inv-redwood', N'2026-07-08', N'Fund III', N'Capital Call', N'#C-2041', -8.2, N'Due'),
    (N'inv-redwood', N'2026-07-03', N'Fund II', N'Distribution', N'#D-119', 2.1, N'Processing'),
    (N'inv-redwood', N'2026-07-01', N'Fund II', N'Capital Call', N'#C-2039', -6.2, N'Funded'),
    (N'inv-redwood', N'2026-06-30', N'Fund III', N'Distribution', N'#D-117', 3.4, N'Paid'),
    (N'inv-redwood', N'2026-03-31', N'Fund II', N'Distribution', N'#D-116', 1.75, N'Paid'),
    (N'inv-redwood', N'2026-03-15', N'Fund III', N'Capital Call', N'#C-2038', -5, N'Funded'),
    (N'inv-redwood', N'2025-12-20', N'Fund III', N'Distribution', N'#D-115', 1.8, N'Paid'),
    (N'inv-redwood', N'2025-11-30', N'Fund II', N'Capital Call', N'#C-2031', -3.5, N'Funded'),
    (N'inv-redwood', N'2025-09-15', N'Fund III', N'Capital Call', N'#C-2025', -9, N'Funded');

IF NOT EXISTS (SELECT 1 FROM [portal].[Documents])
INSERT INTO [portal].[Documents] ([Id], [InvestorId], [Name], [Fund], [Period], [Type], [Date])
VALUES
    (N'doc-1', N'inv-redwood', N'Q2 2026 Capital Account Statement', N'Fund III', N'Q2 2026', N'Capital account', N'2026-07-15'),
    (N'doc-2', N'inv-redwood', N'Q2 2026 Capital Account Statement', N'Fund II', N'Q2 2026', N'Capital account', N'2026-07-15'),
    (N'doc-3', N'inv-redwood', N'Distribution Notice #D-117', N'Fund III', N'Jun 2026', N'Notice', N'2026-06-30'),
    (N'doc-4', N'inv-redwood', N'Capital Call Notice #C-2041', N'Fund III', N'Jul 2026', N'Notice', N'2026-07-01'),
    (N'doc-5', N'inv-redwood', N'2025 Schedule K-1', N'Fund III', N'FY 2025', N'Tax', N'2026-03-20'),
    (N'doc-6', N'inv-redwood', N'2025 Schedule K-1', N'Fund II', N'FY 2025', N'Tax', N'2026-03-20'),
    (N'doc-7', N'inv-redwood', N'Fund III — Q2 2026 Report', N'Fund III', N'Q2 2026', N'Report', N'2026-07-20'),
    (N'doc-8', N'inv-redwood', N'Q1 2026 Capital Account Statement', N'Fund III', N'Q1 2026', N'Capital account', N'2026-04-15');

IF NOT EXISTS (SELECT 1 FROM [portal].[TaxDocuments])
INSERT INTO [portal].[TaxDocuments] ([Id], [InvestorId], [Name], [Fund], [TaxYear], [Type], [Status], [ExpectedDate])
VALUES
    (N'tax-1', N'inv-redwood', N'2025 Schedule K-1', N'Fund III', 2025, N'Federal K-1', N'Available', NULL),
    (N'tax-2', N'inv-redwood', N'2025 Schedule K-1', N'Fund II', 2025, N'Federal K-1', N'Available', NULL),
    (N'tax-3', N'inv-redwood', N'2025 State K-1 (CA)', N'Fund III', 2025, N'State K-1', N'Available', NULL),
    (N'tax-4', N'inv-redwood', N'2024 Schedule K-1', N'Fund III', 2024, N'Federal K-1', N'Available', NULL),
    (N'tax-5', N'inv-redwood', N'2024 Schedule K-1', N'Fund II', 2024, N'Federal K-1', N'Available', NULL),
    (N'tax-6', N'inv-redwood', N'2026 Schedule K-1', N'Fund III', 2026, N'Federal K-1', N'Pending', N'Mar 2027');

IF NOT EXISTS (SELECT 1 FROM [portal].[IrConfig])
INSERT INTO [portal].[IrConfig] ([Id], [ManagerName], [ManagerInitials], [ManagerTitle], [Email], [Phone], [Hours])
VALUES
    (N'current', N'Elena Marsh', N'EM', N'Relationship Manager', N'ir@meridiancredit.com', N'+1 (212) 555-0148', N'Mon–Fri · 9–6 ET');

IF NOT EXISTS (SELECT 1 FROM [portal].[IrRegardingOptions])
INSERT INTO [portal].[IrRegardingOptions] ([Label], [SortOrder])
VALUES
    (N'General enquiry', 1),
    (N'Wire instructions', 2),
    (N'Statements & reporting', 3),
    (N'Tax documents', 4),
    (N'Capital calls & distributions', 5),
    (N'Legal / side letter', 6);

IF NOT EXISTS (SELECT 1 FROM [portal].[IrRequests])
INSERT INTO [portal].[IrRequests] ([InvestorId], [Subject], [Regarding], [Message], [Ref], [Date], [Status])
VALUES
    (N'inv-redwood', N'Wire instruction update', NULL, NULL, N'#REQ-3391', N'2026-06-24', N'Resolved'),
    (N'inv-redwood', N'Q1 statement question', NULL, NULL, N'#REQ-3288', N'2026-04-30', N'Resolved');

