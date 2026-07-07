-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [ops].[CapitalCalls])
INSERT INTO [ops].[CapitalCalls] ([Id], [Ref], [DealId], [DealName], [FundId], [Tranche], [Borrower], [Amount], [DueDate], [Basis], [CurrentStage], [Status], [PendingEscalations], [EscalationGateStage])
VALUES
    (N'call-2041', N'#C-2041', N'deal-atlas', N'Project Atlas', N'fund-iii', N'Term A', N'Vantage Health', 16, N'2026-07-08', N'Unfunded', 4, N'InReview', N'[]', NULL),
    (N'call-2042', N'#C-2042', N'deal-beacon', N'Project Beacon', N'fund-iii', N'Unitranche', N'Nordic Logistics', 26.3, N'2026-07-09', N'Unfunded', 5, N'Pending', N'[]', NULL),
    (N'call-2036', N'#C-2036', N'deal-cedar', N'Project Cedar', N'fund-ii', N'Term B', N'Apex Manufacturing', 6.75, N'2026-07-11', N'Unfunded', 9, N'Completed', N'[]', NULL),
    (N'call-2039', N'#C-2039', N'deal-delta', N'Project Delta', N'fund-ii', N'Term A', N'Helio Software', 18.4, N'2026-07-01', N'Unfunded', 3, N'Returned', N'[]', NULL),
    (N'call-2043', N'#C-2043', N'deal-echo', N'Project Echo', N'fund-iii', N'Term A', N'Coastal Foods', 120, N'2026-08-04', N'Unfunded', 1, N'InReview', N'[]', NULL),
    (N'call-2044', N'#C-2044', N'deal-gale', N'Project Gale', N'fund-iii', N'Mezzanine', N'Harborview REIT', 60.55, N'2026-07-29', N'Unfunded', 2, N'InReview', N'[]', NULL);

IF NOT EXISTS (SELECT 1 FROM [ops].[CallAllocations])
INSERT INTO [ops].[CallAllocations] ([InvestorId], [InvestorName], [Commitment], [Amount], [WireStatus], [CapitalCallId])
VALUES
    (N'inv-redwood', N'Redwood Pension', 40, 8.2, N'Pending', N'call-2041'),
    (N'inv-blueharbor', N'Blue Harbor Endowment', 25, 5.1, N'Wired', N'call-2041'),
    (N'inv-cascade', N'Cascade Family Office', 60, 2.7, N'Scheduled', N'call-2041'),
    (N'inv-cascade', N'Cascade Family Office', 60, 14.4, N'Scheduled', N'call-2042'),
    (N'inv-summit', N'Summit Investments', 50, 11.9, N'Scheduled', N'call-2042'),
    (N'inv-cascade', N'Cascade Family Office', 30, 6.75, N'Confirmed', N'call-2036'),
    (N'inv-redwood', N'Redwood Pension', 18, 6.2, N'Wired', N'call-2039'),
    (N'inv-cascade', N'Cascade Family Office', 30, 5.1, N'Wired', N'call-2039'),
    (N'inv-granite', N'Granite State Insurance', 35, 4.3, N'Overdue', N'call-2039'),
    (N'inv-oakmont', N'Oakmont Trust', 20, 2.8, N'Overdue', N'call-2039'),
    (N'inv-redwood', N'Redwood Pension', 40, 21.8, N'Pending', N'call-2043'),
    (N'inv-blueharbor', N'Blue Harbor Endowment', 25, 13.6, N'Pending', N'call-2043'),
    (N'inv-cascade', N'Cascade Family Office', 60, 32.7, N'Pending', N'call-2043'),
    (N'inv-summit', N'Summit Investments', 50, 27.3, N'Pending', N'call-2043'),
    (N'inv-ironwood', N'Ironwood Capital', 45, 24.6, N'Pending', N'call-2043'),
    (N'inv-redwood', N'Redwood Pension', 40, 11, N'Pending', N'call-2044'),
    (N'inv-blueharbor', N'Blue Harbor Endowment', 25, 6.9, N'Pending', N'call-2044'),
    (N'inv-cascade', N'Cascade Family Office', 60, 16.5, N'Pending', N'call-2044'),
    (N'inv-summit', N'Summit Investments', 50, 13.8, N'Pending', N'call-2044'),
    (N'inv-ironwood', N'Ironwood Capital', 45, 12.35, N'Pending', N'call-2044');

IF NOT EXISTS (SELECT 1 FROM [ops].[CallStageEvents])
INSERT INTO [ops].[CallStageEvents] ([Stage], [State], [Actor], [Date], [Note], [Comment], [CapitalCallId])
VALUES
    (1, N'Done', N'J. Chen', N'2026-07-01', NULL, NULL, N'call-2041'),
    (2, N'Done', N'M. Reyes', N'2026-07-02', NULL, NULL, N'call-2041'),
    (3, N'Done', N'S. Patel', N'2026-07-02', NULL, N'Cleared to proceed', N'call-2041'),
    (4, N'Current', N'J. Okafor', N'2026-07-04', N'In review', NULL, N'call-2041'),
    (1, N'Done', N'J. Chen', N'2026-06-30', NULL, NULL, N'call-2042'),
    (2, N'Done', N'M. Reyes', N'2026-07-01', NULL, NULL, N'call-2042'),
    (3, N'Done', N'S. Patel', N'2026-07-02', NULL, NULL, N'call-2042'),
    (4, N'Done', N'J. Okafor', N'2026-07-03', NULL, NULL, N'call-2042'),
    (5, N'Current', N'T. Alvarez', N'2026-07-04', N'Awaiting review', NULL, N'call-2042'),
    (1, N'Done', N'J. Chen', N'2026-06-24', NULL, NULL, N'call-2036'),
    (2, N'Done', N'M. Reyes', N'2026-06-25', NULL, NULL, N'call-2036'),
    (3, N'Done', N'S. Patel', N'2026-06-25', NULL, NULL, N'call-2036'),
    (4, N'Done', N'J. Okafor', N'2026-06-26', NULL, NULL, N'call-2036'),
    (5, N'Done', N'T. Alvarez', N'2026-06-27', NULL, NULL, N'call-2036'),
    (6, N'Done', N'D. Whitfield', N'2026-06-28', NULL, NULL, N'call-2036'),
    (7, N'Done', N'System', N'2026-06-28', NULL, NULL, N'call-2036'),
    (8, N'Done', N'System', N'2026-06-28', NULL, NULL, N'call-2036'),
    (9, N'Done', N'System', N'2026-06-29', NULL, NULL, N'call-2036'),
    (1, N'Done', N'J. Chen', N'2026-06-20', NULL, NULL, N'call-2039'),
    (2, N'Done', N'M. Reyes', N'2026-06-22', NULL, NULL, N'call-2039'),
    (3, N'Current', N'S. Patel', N'2026-07-04', N'Returned for re-review', NULL, N'call-2039'),
    (1, N'Current', N'J. Chen', N'2026-07-04', N'In review · escalation: >$20M requires CIO + Compliance', NULL, N'call-2043'),
    (1, N'Done', N'J. Chen', N'2026-07-02', NULL, NULL, N'call-2044'),
    (2, N'Current', N'M. Reyes', N'2026-07-03', N'In review · escalation: >$20M requires CIO + Compliance', NULL, N'call-2044');

IF NOT EXISTS (SELECT 1 FROM [ops].[CallDocuments])
INSERT INTO [ops].[CallDocuments] ([Name], [By], [Date], [CapitalCallId])
VALUES
    (N'Capital Call Notice.pdf', N'J. Chen', N'2026-07-01', N'call-2041'),
    (N'Wire Instructions.pdf', N'Operations', N'2026-07-01', N'call-2041'),
    (N'LPA Excerpt — §4.2.pdf', N'Legal', N'2026-07-04', N'call-2041'),
    (N'Capital Call Notice.pdf', N'J. Chen', N'2026-06-30', N'call-2042'),
    (N'Wire Instructions.pdf', N'Operations', N'2026-06-30', N'call-2042'),
    (N'Capital Call Notice.pdf', N'J. Chen', N'2026-06-24', N'call-2036'),
    (N'Capital Call Notice.pdf', N'J. Chen', N'2026-06-20', N'call-2039'),
    (N'Allocation Memo (rev 2).pdf', N'D. Whitfield', N'2026-07-04', N'call-2039'),
    (N'Capital Call Notice (draft).pdf', N'J. Chen', N'2026-07-04', N'call-2043'),
    (N'Capital Call Notice.pdf', N'J. Chen', N'2026-07-02', N'call-2044');

IF NOT EXISTS (SELECT 1 FROM [ops].[CallAuditEntries])
INSERT INTO [ops].[CallAuditEntries] ([Title], [By], [At], [Comment], [Tone], [CapitalCallId])
VALUES
    (N'Legal review started', N'J. Okafor', N'2026-07-04T10:12:00', NULL, N'blue', N'call-2041'),
    (N'CIO approved', N'S. Patel', N'2026-07-02T16:40:00', N'Cleared to proceed', N'green', N'call-2041'),
    (N'Front Office approved', N'M. Reyes', N'2026-07-02T09:15:00', NULL, N'green', N'call-2041'),
    (N'Submitted for review', N'J. Chen', N'2026-07-01T14:02:00', NULL, N'green', N'call-2041'),
    (N'Call created', N'System', N'2026-07-01T08:00:00', NULL, N'neutral', N'call-2041'),
    (N'Legal approved', N'J. Okafor', N'2026-07-03T15:20:00', NULL, N'green', N'call-2042'),
    (N'CIO approved', N'S. Patel', N'2026-07-02T11:05:00', NULL, N'green', N'call-2042'),
    (N'Front Office approved', N'M. Reyes', N'2026-07-01T10:30:00', NULL, N'green', N'call-2042'),
    (N'Submitted for review', N'J. Chen', N'2026-06-30T09:12:00', NULL, N'green', N'call-2042'),
    (N'Call created', N'System', N'2026-06-30T08:00:00', NULL, N'neutral', N'call-2042'),
    (N'Call completed', N'System', N'2026-06-29T08:00:00', NULL, N'green', N'call-2036'),
    (N'Custodians notified', N'System', N'2026-06-28T17:20:00', NULL, N'green', N'call-2036'),
    (N'Booked to GL', N'System', N'2026-06-28T16:05:00', NULL, N'green', N'call-2036'),
    (N'Call created', N'System', N'2026-06-24T08:00:00', NULL, N'neutral', N'call-2036'),
    (N'Returned to CIO', N'J. Okafor', N'2026-07-04T14:22:00', N'Allocation revision needs CIO re-approval', N'amber', N'call-2039'),
    (N'Allocation edited', N'D. Whitfield', N'2026-07-04T10:15:00', N'$5.10M → $5.00M correction reversed', N'amber', N'call-2039'),
    (N'CIO approved', N'S. Patel', N'2026-06-23T12:00:00', NULL, N'green', N'call-2039'),
    (N'Call created', N'System', N'2026-06-20T08:00:00', NULL, N'neutral', N'call-2039'),
    (N'Call created', N'J. Chen', N'2026-07-04T09:30:00', NULL, N'neutral', N'call-2043'),
    (N'Operations approved', N'J. Chen', N'2026-07-02T15:45:00', NULL, N'green', N'call-2044'),
    (N'Call created', N'System', N'2026-07-02T08:00:00', NULL, N'neutral', N'call-2044');

