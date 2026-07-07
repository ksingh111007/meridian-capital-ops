-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.

IF NOT EXISTS (SELECT 1 FROM [audit].[Events])
BEGIN
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-03T15:40:00', N'S. Patel', N'Config saved', N'amber', N'Workflow “Capital Calls”', N'9 stages', N'04a2cff6cfd6');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-04T10:15:00', N'D. Whitfield', N'Edited alloc.', N'amber', N'Call #C-2039', N'$5.10M → $5.00M', N'd460d03bcde5');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-04T11:02:00', N'Admin', N'Role changed', N'amber', N'user maria.reyes', N'→ Deal Lead', N'332b48f441b8');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-04T16:20:00', N'J. Okafor', N'Started review', N'blue', N'Call #C-2041 · Legal', N'stage 4 of 9', N'6c4a80ace116');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-05T09:14:00', N'T. Alvarez', N'Marked wired', N'blue', N'Dist #D-119 · Redwood', N'$2.10M', N'0c1ef897fda3');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-05T09:31:00', N'System', N'Wire exception', N'red', N'W-8847', N'SWIFT reject · MT103', N'88294c2567dd');
    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal])
    VALUES (N'2026-07-05T09:41:00', N'S. Patel', N'Approved', N'green', N'Call #C-2041 · CIO', N'“Cleared to proceed”', N'572f65dcfe65');
END;

