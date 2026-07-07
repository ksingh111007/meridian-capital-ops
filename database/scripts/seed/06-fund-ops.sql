-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [ops].[Drawdowns])
INSERT INTO [ops].[Drawdowns] ([Id], [Facility], [Lender], [Purpose], [DealId], [LinkedCallId], [Amount], [Rate], [DrawDate], [RepayBy], [Status])
VALUES
    (N'draw-1', N'Sub Line A', N'Silverpoint Bank', N'Bridge — Project Atlas', N'deal-atlas', N'call-2041', 16, N'S+2.50%', N'2026-07-02', N'2026-08-15', N'Outstanding'),
    (N'draw-2', N'Sub Line A', N'Silverpoint Bank', N'Bridge — Project Beacon', N'deal-beacon', N'call-2042', 26.3, N'S+2.50%', N'2026-07-03', N'2026-08-20', N'Outstanding'),
    (N'draw-3', N'NAV Facility', N'Meridian Credit Partners', N'Portfolio — Fund III', NULL, NULL, 120, N'S+3.00%', N'2026-06-15', N'2026-12-31', N'Outstanding'),
    (N'draw-4', N'Sub Line B', N'Harbor Trust', N'Bridge — Project Cedar', N'deal-cedar', N'call-2036', 6.75, N'S+2.75%', N'2026-06-28', NULL, N'Repaid'),
    (N'draw-5', N'Sub Line A', N'Silverpoint Bank', N'Bridge — Project Echo', N'deal-echo', N'call-2043', 15, N'S+2.50%', N'2026-07-04', N'2026-08-30', N'Requested');

IF NOT EXISTS (SELECT 1 FROM [ops].[Wires])
INSERT INTO [ops].[Wires] ([Id], [Ref], [Direction], [Counterparty], [Type], [LinkedRef], [Amount], [Time], [Date], [Rail], [Status], [ExceptionReason])
VALUES
    (N'wire-8842', N'W-8842', N'In', N'Blue Harbor Endowment', N'Capital Call', N'#C-2041', 5.1, N'09:02', N'2026-07-05', N'Fedwire', N'Settled', NULL),
    (N'wire-8843', N'W-8843', N'Out', N'Redwood Pension', N'Distribution', N'#D-119', 2.1, N'09:14', N'2026-07-05', N'Fedwire', N'Sent', NULL),
    (N'wire-8844', N'W-8844', N'Out', N'Cascade Family Office', N'Distribution', N'#D-119', 3.5, N'09:15', N'2026-07-05', N'Fedwire', N'Settled', NULL),
    (N'wire-8845', N'W-8845', N'In', N'Cascade Family Office', N'Capital Call', N'#C-2041', 2.7, N'09:20', N'2026-07-05', N'ACH', N'Queued', NULL),
    (N'wire-8846', N'W-8846', N'Out', N'Silverpoint Bank', N'Facility Repay', N'Sub Line B', 6.75, N'09:22', N'2026-07-05', N'Fedwire', N'Acknowledged', NULL),
    (N'wire-8847', N'W-8847', N'Out', N'Granite State Insurance', N'Distribution', N'#D-119', 4.08, N'09:31', N'2026-07-05', N'SWIFT', N'Exception', N'SWIFT reject · MT103 — signing certificate invalid');

IF NOT EXISTS (SELECT 1 FROM [ops].[ReconItems])
INSERT INTO [ops].[ReconItems] ([Id], [Date], [Description], [Source], [Book], [Custodian], [Diff], [Status], [Assignee])
VALUES
    (N'rec-1', N'2026-07-04', N'Redwood — call receipt (#C-2039)', N'Northern Trust', 6.2, 6.2, 0, N'Matched', NULL),
    (N'rec-2', N'2026-07-04', N'Beacon — facility draw', N'Silverpoint', 26.3, 26.3, 0, N'Matched', NULL),
    (N'rec-3', N'2026-07-03', N'Distribution #D-119 batch — partial settlement', N'Northern Trust', 12, 11.58, 0.42, N'Break', NULL),
    (N'rec-4', N'2026-07-03', N'Mgmt fee accrual', N'GL / Book', 1.04, NULL, 1.04, N'Unmatched', NULL),
    (N'rec-5', N'2026-07-02', N'Interest income — Vantage', N'Northern Trust', 0.66, 0.66, 0, N'Matched', NULL);

IF NOT EXISTS (SELECT 1 FROM [ops].[CashAccounts])
INSERT INTO [ops].[CashAccounts] ([Custodian], [Account], [Currency], [Type], [Balance])
VALUES
    (N'Northern Trust', N'Fund III — Operating', N'USD', N'Operating', 58.2),
    (N'Northern Trust', N'Fund III — Subscription', N'USD', N'Escrow', 22),
    (N'State Street', N'Fund III — Reserve', N'USD', N'Reserve', 12.2);

IF NOT EXISTS (SELECT 1 FROM [ops].[CashPositionSnapshots])
INSERT INTO [ops].[CashPositionSnapshots] ([Id], [AsOf], [FundId], [CashOnHand], [AccountsCount], [UncalledCapital], [UncalledLps], [FacilityHeadroom], [FacilityLimit], [Net30DayProjection], [CoverageRatio])
VALUES
    (N'current', N'2026-07-05', N'fund-iii', 92.4, 3, 512, 48, 116, 300, -13, 1.4);

IF NOT EXISTS (SELECT 1 FROM [ops].[CashForecastBars])
INSERT INTO [ops].[CashForecastBars] ([SortOrder], [Height], [CashPositionSnapshotId])
VALUES
    (1, 74, N'current'),
    (2, 52, N'current'),
    (3, 63, N'current'),
    (4, 44, N'current'),
    (5, 58, N'current'),
    (6, 70, N'current'),
    (7, 66, N'current'),
    (8, 80, N'current'),
    (9, 72, N'current'),
    (10, 68, N'current'),
    (11, 75, N'current'),
    (12, 71, N'current'),
    (13, 78, N'current');

IF NOT EXISTS (SELECT 1 FROM [ops].[CashForecastWeeks])
INSERT INTO [ops].[CashForecastWeeks] ([SortOrder], [Label], [Inflows], [Outflows], [Net], [ProjectedBalance], [CashPositionSnapshotId])
VALUES
    (1, N'Jul 07–11', 27.3, -45.9, -18.6, 73.8, N'current'),
    (2, N'Jul 14–18', 16, -6.8, 9.2, 83, N'current'),
    (3, N'Jul 21–25', 8.4, 0, 8.4, 91.4, N'current'),
    (4, N'Jul 28–Aug 1', 0, -12, -12, 79.4, N'current');

IF NOT EXISTS (SELECT 1 FROM [ops].[PortfolioSnapshots])
INSERT INTO [ops].[PortfolioSnapshots] ([Id], [AsOf], [InvestedCapital], [ActiveDeals], [NetIrrPct], [BlendedMoic], [OnWatchCount], [OnWatchExposure], [PerformingPct], [WatchPct], [NonAccrualPct])
VALUES
    (N'current', N'2026-07-05', 520, 7, 14.2, 1.19, 2, 58, 84, 9, 7);

IF NOT EXISTS (SELECT 1 FROM [ops].[PortfolioTrendPoints])
INSERT INTO [ops].[PortfolioTrendPoints] ([SortOrder], [Value], [PortfolioSnapshotId])
VALUES
    (1, 48, N'current'),
    (2, 55, N'current'),
    (3, 53, N'current'),
    (4, 64, N'current'),
    (5, 70, N'current'),
    (6, 67, N'current'),
    (7, 76, N'current'),
    (8, 82, N'current');

