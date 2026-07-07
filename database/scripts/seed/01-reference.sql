-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [ref].[Funds])
INSERT INTO [ref].[Funds] ([Id], [Name], [ShortName], [Vintage], [Committed], [CalledPct], [Strategy], [WaterfallType], [BaseCurrency], [Status])
VALUES
    (N'fund-iii', N'Meridian Credit III', N'Fund III', 2024, 1200, 71, N'Direct Lending', N'European', N'USD', N'Active'),
    (N'fund-ii', N'Meridian Credit II', N'Fund II', 2021, 800, 96, N'Direct Lending', N'European', N'USD', N'Investing'),
    (N'fund-i', N'Meridian Credit I', N'Fund I', 2018, 450, 100, N'Direct Lending', N'American', N'USD', N'Harvesting');

IF NOT EXISTS (SELECT 1 FROM [ref].[LegalEntities])
INSERT INTO [ref].[LegalEntities] ([FundId], [Name], [Kind])
VALUES
    (N'fund-iii', N'Meridian Credit III GP, LLC', N'GP'),
    (N'fund-iii', N'Meridian Credit III Master, LP', N'Master'),
    (N'fund-iii', N'MC III Onshore Feeder, LP', N'Feeder'),
    (N'fund-iii', N'MC III Offshore Feeder', N'Cayman'),
    (N'fund-iii', N'MC III Blocker Corp', N'Blocker');

IF NOT EXISTS (SELECT 1 FROM [ref].[ShareClasses])
INSERT INTO [ref].[ShareClasses] ([FundId], [Name], [MgmtFeePct], [CarryPct], [PrefPct])
VALUES
    (N'fund-iii', N'Class A', 1.5, 20, 8),
    (N'fund-iii', N'Class B', 1.25, 15, 8);

IF NOT EXISTS (SELECT 1 FROM [ref].[Deals])
INSERT INTO [ref].[Deals] ([Id], [Name], [Borrower], [Sector], [Country], [FundId], [Tranche], [Invested], [Outstanding], [Spread], [NetIrrPct], [IrrTrend], [Moic], [Status])
VALUES
    (N'deal-atlas', N'Project Atlas', N'Vantage Health', N'Healthcare', N'US', N'fund-iii', N'Term A', 85, 80, N'S+2.50%', 13.8, N'up', 1.12, N'Performing'),
    (N'deal-beacon', N'Project Beacon', N'Nordic Logistics', N'Transport & Logistics', N'SE', N'fund-iii', N'Unitranche', 110, 104, N'S+2.50%', 15.1, N'up', 1.21, N'Performing'),
    (N'deal-cedar', N'Project Cedar', N'Apex Manufacturing', N'Industrials', N'US', N'fund-ii', N'Term B', 60, 22, N'S+2.75%', 12.4, N'flat', 1.34, N'Performing'),
    (N'deal-delta', N'Project Delta', N'Helio Software', N'Technology', N'US', N'fund-ii', N'Term A', 48, 40, N'S+3.25%', 9.6, N'down', 1.08, N'Watch'),
    (N'deal-echo', N'Project Echo', N'Coastal Foods', N'Consumer Staples', N'US', N'fund-iii', N'Term A', 95, 88, N'S+2.50%', 16.2, N'up', 1.19, N'Performing'),
    (N'deal-foxtrot', N'Project Foxtrot', N'Summit Metals', N'Materials', N'US', N'fund-ii', N'Term B', 52, 18, N'S+3.00%', 7.1, N'down', 0.94, N'Non-accrual'),
    (N'deal-gale', N'Project Gale', N'Harborview REIT', N'Real Estate', N'US', N'fund-iii', N'Mezzanine', 70, 66, N'S+4.00%', 18.4, N'up', 1.27, N'Performing');

IF NOT EXISTS (SELECT 1 FROM [ref].[DealDetails])
INSERT INTO [ref].[DealDetails] ([DealId], [FairValue], [Facility], [Drawn], [Maturity], [SpreadFloor], [UpfrontFeePct], [InternalRating], [RiskTrend], [Covenants], [NetLeverage], [LastReview])
VALUES
    (N'deal-atlas', 86.1, 100, 80, N'Mar 2028', N'SOFR + 2.50% · 0.75%', 1.75, N'B+', N'Stable', N'In compliance', N'3.8×', N'May 2026'),
    (N'deal-beacon', 112.4, 110, 104, N'Jun 2029', N'SOFR + 2.50% · 1.00%', 2, N'BB−', N'Stable', N'In compliance', N'4.2×', N'Jun 2026'),
    (N'deal-cedar', 24.6, 60, 22, N'Sep 2027', N'SOFR + 2.75% · 1.00%', 2, N'B', N'Improving', N'In compliance', N'3.1×', N'Apr 2026'),
    (N'deal-delta', 38.7, 48, 40, N'Dec 2027', N'SOFR + 3.25% · 1.00%', 2.25, N'B−', N'Deteriorating', N'Leverage waiver in place', N'6.1×', N'Jun 2026'),
    (N'deal-echo', 96.8, 95, 88, N'Feb 2030', N'SOFR + 2.50% · 0.75%', 1.5, N'BB−', N'Stable', N'In compliance', N'4.0×', N'Jun 2026'),
    (N'deal-foxtrot', 14.9, 52, 18, N'Jun 2026 (extended)', N'SOFR + 3.00% · 1.00%', 2.5, N'CCC', N'Deteriorating', N'In default — forbearance', N'8.4×', N'Jul 2026'),
    (N'deal-gale', 72.9, 70, 66, N'Oct 2028', N'SOFR + 4.00% · 1.25%', 2, N'B+', N'Stable', N'In compliance', N'n/a (asset-backed)', N'May 2026');

IF NOT EXISTS (SELECT 1 FROM [ref].[DealCashflows])
INSERT INTO [ref].[DealCashflows] ([Date], [Type], [Amount], [PrincipalBalance], [DealDetailDealId])
VALUES
    (N'2026-06-30', N'Interest', 1.02, 80, N'deal-atlas'),
    (N'2026-03-31', N'Interest', 1.02, 80, N'deal-atlas'),
    (N'2025-11-14', N'Draw', -20, 80, N'deal-atlas'),
    (N'2025-06-20', N'Initial draw', -60, 60, N'deal-atlas'),
    (N'2026-06-30', N'Interest', 1.31, 104, N'deal-beacon'),
    (N'2026-04-12', N'Draw', -14, 104, N'deal-beacon'),
    (N'2026-03-31', N'Interest', 1.13, 90, N'deal-beacon'),
    (N'2025-12-31', N'Interest', 1.13, 90, N'deal-beacon'),
    (N'2025-09-15', N'Initial draw', -90, 90, N'deal-beacon'),
    (N'2026-06-30', N'Interest', 0.42, 22, N'deal-cedar'),
    (N'2026-02-15', N'Repayment', 18, 22, N'deal-cedar'),
    (N'2024-08-01', N'Initial draw', -40, 40, N'deal-cedar'),
    (N'2026-06-30', N'Interest', 0.71, 40, N'deal-delta'),
    (N'2026-03-31', N'Interest (PIK)', 0, 40, N'deal-delta'),
    (N'2023-11-30', N'Initial draw', -40, 40, N'deal-delta'),
    (N'2026-06-30', N'Interest', 1.12, 88, N'deal-echo'),
    (N'2025-02-28', N'Initial draw', -88, 88, N'deal-echo'),
    (N'2026-06-30', N'Interest (unpaid)', 0, 18, N'deal-foxtrot'),
    (N'2025-12-15', N'Partial repayment', 12, 18, N'deal-foxtrot'),
    (N'2022-05-10', N'Initial draw', -30, 30, N'deal-foxtrot'),
    (N'2026-06-30', N'Interest', 1.55, 66, N'deal-gale'),
    (N'2025-10-20', N'Initial draw', -66, 66, N'deal-gale');

IF NOT EXISTS (SELECT 1 FROM [ref].[DealLpExposures])
INSERT INTO [ref].[DealLpExposures] ([Investor], [Amount], [DealDetailDealId])
VALUES
    (N'Redwood Pension', 15.5, N'deal-atlas'),
    (N'Cascade Family Office', 23.2, N'deal-atlas'),
    (N'Cascade Family Office', 14.4, N'deal-beacon'),
    (N'Summit Investments', 11.9, N'deal-beacon'),
    (N'Cascade Family Office', 8.2, N'deal-cedar'),
    (N'Granite State Insurance', 9.6, N'deal-cedar'),
    (N'Redwood Pension', 9, N'deal-delta'),
    (N'Oakmont Trust', 5, N'deal-delta'),
    (N'Summit Investments', 20, N'deal-echo'),
    (N'Ironwood Capital', 18, N'deal-echo'),
    (N'Granite State Insurance', 7.9, N'deal-foxtrot'),
    (N'Cascade Family Office', 6.1, N'deal-foxtrot'),
    (N'Ironwood Capital', 14.2, N'deal-gale'),
    (N'Blue Harbor Endowment', 8.8, N'deal-gale');

IF NOT EXISTS (SELECT 1 FROM [ref].[DealDocuments])
INSERT INTO [ref].[DealDocuments] ([Name], [DealDetailDealId])
VALUES
    (N'Credit Agreement.pdf', N'deal-atlas'),
    (N'Q2 Compliance Cert.pdf', N'deal-atlas'),
    (N'Credit Agreement.pdf', N'deal-beacon'),
    (N'Amendment No. 1.pdf', N'deal-beacon'),
    (N'Q2 Compliance Cert.pdf', N'deal-beacon'),
    (N'Credit Agreement.pdf', N'deal-cedar'),
    (N'Credit Agreement.pdf', N'deal-delta'),
    (N'Waiver Letter — Q2.pdf', N'deal-delta'),
    (N'Credit Agreement.pdf', N'deal-echo'),
    (N'Q2 Compliance Cert.pdf', N'deal-echo'),
    (N'Credit Agreement.pdf', N'deal-foxtrot'),
    (N'Forbearance Agreement.pdf', N'deal-foxtrot'),
    (N'Restructuring Memo.pdf', N'deal-foxtrot'),
    (N'Credit Agreement.pdf', N'deal-gale'),
    (N'Appraisal — 2026.pdf', N'deal-gale');

IF NOT EXISTS (SELECT 1 FROM [ref].[Investors])
INSERT INTO [ref].[Investors] ([Id], [Name], [Type], [KycStatus], [WireInstructionsOnFile])
VALUES
    (N'inv-redwood', N'Redwood Pension', N'Public Pension', N'Verified', 1),
    (N'inv-blueharbor', N'Blue Harbor Endowment', N'Endowment', N'Verified', 1),
    (N'inv-cascade', N'Cascade Family Office', N'Family Office', N'Verified', 1),
    (N'inv-granite', N'Granite State Insurance', N'Insurance', N'In review', 1),
    (N'inv-summit', N'Summit Investments', N'Fund of Funds', N'Verified', 1),
    (N'inv-oakmont', N'Oakmont Trust', N'Private Trust', N'Verified', 0),
    (N'inv-ironwood', N'Ironwood Capital', N'Asset Manager', N'Verified', 1);

IF NOT EXISTS (SELECT 1 FROM [ref].[InvestorCommitments])
INSERT INTO [ref].[InvestorCommitments] ([FundId], [Amount], [Called], [InvestorId])
VALUES
    (N'fund-iii', 40, 28.4, N'inv-redwood'),
    (N'fund-ii', 18, 17.3, N'inv-redwood'),
    (N'fund-iii', 25, 17, N'inv-blueharbor'),
    (N'fund-iii', 60, 42.9, N'inv-cascade'),
    (N'fund-ii', 30, 28.8, N'inv-cascade'),
    (N'fund-ii', 35, 33.6, N'inv-granite'),
    (N'fund-iii', 50, 36, N'inv-summit'),
    (N'fund-ii', 20, 19.2, N'inv-oakmont'),
    (N'fund-iii', 45, 31.5, N'inv-ironwood');

IF NOT EXISTS (SELECT 1 FROM [ref].[InvestorProfiles])
INSERT INTO [ref].[InvestorProfiles] ([InvestorId], [Bank], [AbaMasked], [AccountMasked], [BankingVerified], [KycDocs], [KycReviewDue])
VALUES
    (N'inv-redwood', N'Northern Trust', N'•••• 0021', N'•••• 4471', N'Jun 2026', N'W-9, LPA, entity', N'Jun 2027');

IF NOT EXISTS (SELECT 1 FROM [ref].[Borrowers])
INSERT INTO [ref].[Borrowers] ([Name], [Sector], [Country], [DealName], [InternalRating])
VALUES
    (N'Vantage Health', N'Healthcare', N'US', N'Project Atlas', N'B+'),
    (N'Nordic Logistics', N'Transport & Logistics', N'SE', N'Project Beacon', N'BB−'),
    (N'Apex Manufacturing', N'Industrials', N'US', N'Project Cedar', N'B'),
    (N'Helio Software', N'Technology', N'US', N'Project Delta', N'B−'),
    (N'Coastal Foods', N'Consumer Staples', N'US', N'Project Echo', N'BB−'),
    (N'Summit Metals', N'Materials', N'US', N'Project Foxtrot', N'CCC'),
    (N'Harborview REIT', N'Real Estate', N'US', N'Project Gale', N'B+');

IF NOT EXISTS (SELECT 1 FROM [ref].[CurrencyRates])
INSERT INTO [ref].[CurrencyRates] ([Code], [Rate], [Note])
VALUES
    (N'USD', 1, N'base'),
    (N'EUR', 1.085, N''),
    (N'GBP', 1.271, N'');

IF NOT EXISTS (SELECT 1 FROM [ref].[SettlementCalendars])
INSERT INTO [ref].[SettlementCalendars] ([Name], [NextHoliday])
VALUES
    (N'US (SIFMA)', N'Sep 07'),
    (N'UK', N'Aug 25'),
    (N'TARGET2 (EUR)', N'Dec 25');

