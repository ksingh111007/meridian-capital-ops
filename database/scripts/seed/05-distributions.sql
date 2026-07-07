-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [ops].[Distributions])
INSERT INTO [ops].[Distributions] ([Id], [Ref], [FundId], [Distributable], [LpTotal], [GpTotal], [PaymentDate], [Status], [WaterfallType], [SourceNote], [Recallable])
VALUES
    (N'dist-118', N'#D-118', N'fund-iii', 42, 39.2, 2.8, N'2026-09-30', N'Scheduled', N'European', N'Loan repayments + interest', 0),
    (N'dist-119', N'#D-119', N'fund-ii', 12.75, 12, 0.75, N'2026-07-03', N'Paying', N'European', N'Cedar repayment + interest', 0),
    (N'dist-117', N'#D-117', N'fund-iii', 19.9, 18.7, 1.2, N'2026-06-30', N'Paid', N'European', N'Interest income', 0),
    (N'dist-116', N'#D-116', N'fund-ii', 10.6, 10, 0.6, N'2026-03-31', N'Paid', N'European', N'Interest income', 0);

IF NOT EXISTS (SELECT 1 FROM [ops].[DistributionTiers])
INSERT INTO [ops].[DistributionTiers] ([Tier], [Basis], [Rate], [Distributed], [LpShare], [GpShare], [PoolLeft], [DistributionId])
VALUES
    (N'1 · Return of Capital', N'Contributed capital', N'100% LP', 28, 28, NULL, 14, N'dist-118'),
    (N'2 · Preferred Return', N'Hurdle on capital', N'8.0%', 6, 6, NULL, 8, N'dist-118'),
    (N'3 · GP Catch-up', N'Until GP = 20% of profit', N'100% GP', 1.5, NULL, 1.5, 6.5, N'dist-118'),
    (N'4 · Carried Interest', N'Residual profit split', N'80 / 20', 6.5, 5.2, 1.3, 0, N'dist-118'),
    (N'1 · Return of Capital', N'Contributed capital', N'100% LP', 9, 9, NULL, 3.75, N'dist-119'),
    (N'2 · Preferred Return', N'Hurdle on capital', N'8.0%', 2, 2, NULL, 1.75, N'dist-119'),
    (N'3 · GP Catch-up', N'Until GP = 20% of profit', N'100% GP', 0.5, NULL, 0.5, 1.25, N'dist-119'),
    (N'4 · Carried Interest', N'Residual profit split', N'80 / 20', 1.25, 1, 0.25, 0, N'dist-119'),
    (N'1 · Return of Capital', N'Contributed capital', N'100% LP', 14, 14, NULL, 5.9, N'dist-117'),
    (N'2 · Preferred Return', N'Hurdle on capital', N'8.0%', 2.9, 2.9, NULL, 3, N'dist-117'),
    (N'3 · GP Catch-up', N'Until GP = 20% of profit', N'100% GP', 0.75, NULL, 0.75, 2.25, N'dist-117'),
    (N'4 · Carried Interest', N'Residual profit split', N'80 / 20', 2.25, 1.8, 0.45, 0, N'dist-117'),
    (N'1 · Return of Capital', N'Contributed capital', N'100% LP', 7.5, 7.5, NULL, 3.1, N'dist-116'),
    (N'2 · Preferred Return', N'Hurdle on capital', N'8.0%', 1.7, 1.7, NULL, 1.4, N'dist-116'),
    (N'3 · GP Catch-up', N'Until GP = 20% of profit', N'100% GP', 0.4, NULL, 0.4, 1, N'dist-116'),
    (N'4 · Carried Interest', N'Residual profit split', N'80 / 20', 1, 0.8, 0.2, 0, N'dist-116');

IF NOT EXISTS (SELECT 1 FROM [ops].[DistributionPayouts])
INSERT INTO [ops].[DistributionPayouts] ([InvestorId], [InvestorName], [Commitment], [Amount], [PctOfLpTotal], [Status], [BlockedReason], [WireRef], [DistributionId])
VALUES
    (N'inv-redwood', N'Redwood Pension', 40, 7.13, 18.2, N'Scheduled', NULL, NULL, N'dist-118'),
    (N'inv-blueharbor', N'Blue Harbor Endowment', 25, 4.45, 11.4, N'Scheduled', NULL, NULL, N'dist-118'),
    (N'inv-cascade', N'Cascade Family Office', 60, 10.69, 27.3, N'Scheduled', NULL, NULL, N'dist-118'),
    (N'inv-summit', N'Summit Investments', 50, 8.91, 22.7, N'Scheduled', NULL, NULL, N'dist-118'),
    (N'inv-ironwood', N'Ironwood Capital', 45, 8.02, 20.4, N'Scheduled', NULL, NULL, N'dist-118'),
    (N'inv-redwood', N'Redwood Pension', 18, 2.1, 17.5, N'Sent', NULL, N'W-8843', N'dist-119'),
    (N'inv-cascade', N'Cascade Family Office', 30, 3.5, 29.2, N'Paid', NULL, N'W-8844', N'dist-119'),
    (N'inv-granite', N'Granite State Insurance', 35, 4.08, 34, N'Exception', NULL, N'W-8847', N'dist-119'),
    (N'inv-oakmont', N'Oakmont Trust', 20, 2.32, 19.3, N'Blocked', N'No wire instructions on file', NULL, N'dist-119'),
    (N'inv-redwood', N'Redwood Pension', 40, 3.4, 18.2, N'Paid', NULL, NULL, N'dist-117'),
    (N'inv-blueharbor', N'Blue Harbor Endowment', 25, 2.13, 11.4, N'Paid', NULL, NULL, N'dist-117'),
    (N'inv-cascade', N'Cascade Family Office', 60, 5.1, 27.3, N'Paid', NULL, NULL, N'dist-117'),
    (N'inv-summit', N'Summit Investments', 50, 4.25, 22.7, N'Paid', NULL, NULL, N'dist-117'),
    (N'inv-ironwood', N'Ironwood Capital', 45, 3.82, 20.4, N'Paid', NULL, NULL, N'dist-117'),
    (N'inv-redwood', N'Redwood Pension', 18, 1.75, 17.5, N'Paid', NULL, NULL, N'dist-116'),
    (N'inv-cascade', N'Cascade Family Office', 30, 2.91, 29.1, N'Paid', NULL, NULL, N'dist-116'),
    (N'inv-granite', N'Granite State Insurance', 35, 3.4, 34, N'Paid', NULL, NULL, N'dist-116'),
    (N'inv-oakmont', N'Oakmont Trust', 20, 1.94, 19.4, N'Paid', NULL, NULL, N'dist-116');

