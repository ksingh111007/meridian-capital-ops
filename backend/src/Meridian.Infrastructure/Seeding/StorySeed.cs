using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Seeding;

/// <summary>
/// Deterministic sample data mirroring the frontend mock story 1:1
/// (meridian-capital-ops/src/mocks + docs/DATA_MODEL.md § Cross-screen consistency):
/// #C-2041 sits at Legal awaiting J. Okafor, #C-2039 is Returned at CIO with two
/// overdue wires, #C-2043/#C-2044 are >$20M calls awaiting CIO + Compliance
/// sign-off, #D-119 has Oakmont Blocked (no wire instructions) and Granite in
/// Exception. Business "today" is pinned to 2026-07-05 in dev. When editing, keep
/// every value consistent with the JSON files in meridian-capital-ops/src/mocks —
/// the frontend swap depends on both sources telling the same story.
/// </summary>
public static class StorySeed
{
    public static void Apply(AppDbContext db)
    {
        db.Funds.AddRange(Funds());
        db.Deals.AddRange(Deals());
        db.Investors.AddRange(Investors());
        db.WorkflowStages.AddRange(WorkflowStages());
        db.EscalationRules.AddRange(EscalationRules());
        db.Roles.AddRange(Roles());
        db.StaffUsers.AddRange(StaffUsers());
        db.CapitalCalls.AddRange(CapitalCalls());
        db.Distributions.AddRange(Distributions());
        db.AuditEvents.AddRange(AuditEvents());
    }

    private static DateOnly Day(int month, int day) => new(2026, month, day);

    private static DateTime At(int month, int day, int hour, int minute) =>
        new(2026, month, day, hour, minute, 0, DateTimeKind.Utc);

    private static List<Fund> Funds() =>
    [
        new() { Id = "fund-iii", Name = "Meridian Private Credit Fund III", ShortName = "Fund III", Vintage = 2023, Committed = 400m, CalledPct = 62m, Strategy = "Senior direct lending", WaterfallType = WaterfallType.European, Status = FundStatus.Investing },
        new() { Id = "fund-ii", Name = "Meridian Private Credit Fund II", ShortName = "Fund II", Vintage = 2019, Committed = 250m, CalledPct = 91m, Strategy = "Senior direct lending", WaterfallType = WaterfallType.European, Status = FundStatus.Harvesting },
        new() { Id = "fund-i", Name = "Meridian Private Credit Fund I", ShortName = "Fund I", Vintage = 2016, Committed = 120m, CalledPct = 100m, Strategy = "Opportunistic credit", WaterfallType = WaterfallType.American, Status = FundStatus.Harvesting },
    ];

    // Mirrors src/mocks/deals.json exactly.
    private static List<Deal> Deals() =>
    [
        new() { Id = "deal-atlas", Name = "Project Atlas", Borrower = "Vantage Health", Sector = "Healthcare", Country = "US", FundId = "fund-iii", Tranche = "Term A", Invested = 85m, Outstanding = 80m, Spread = "S+2.50%", NetIrrPct = 13.8m, IrrTrend = "up", Moic = 1.12m, Status = "Performing" },
        new() { Id = "deal-beacon", Name = "Project Beacon", Borrower = "Nordic Logistics", Sector = "Transport & Logistics", Country = "SE", FundId = "fund-iii", Tranche = "Unitranche", Invested = 110m, Outstanding = 104m, Spread = "S+2.50%", NetIrrPct = 15.1m, IrrTrend = "up", Moic = 1.21m, Status = "Performing" },
        new() { Id = "deal-cedar", Name = "Project Cedar", Borrower = "Apex Manufacturing", Sector = "Industrials", Country = "US", FundId = "fund-ii", Tranche = "Term B", Invested = 60m, Outstanding = 22m, Spread = "S+2.75%", NetIrrPct = 12.4m, IrrTrend = "flat", Moic = 1.34m, Status = "Performing" },
        new() { Id = "deal-delta", Name = "Project Delta", Borrower = "Helio Software", Sector = "Technology", Country = "US", FundId = "fund-ii", Tranche = "Term A", Invested = 48m, Outstanding = 40m, Spread = "S+3.25%", NetIrrPct = 9.6m, IrrTrend = "down", Moic = 1.08m, Status = "Watch" },
        new() { Id = "deal-echo", Name = "Project Echo", Borrower = "Coastal Foods", Sector = "Consumer Staples", Country = "US", FundId = "fund-iii", Tranche = "Term A", Invested = 95m, Outstanding = 88m, Spread = "S+2.50%", NetIrrPct = 16.2m, IrrTrend = "up", Moic = 1.19m, Status = "Performing" },
        new() { Id = "deal-foxtrot", Name = "Project Foxtrot", Borrower = "Summit Metals", Sector = "Materials", Country = "US", FundId = "fund-ii", Tranche = "Term B", Invested = 52m, Outstanding = 18m, Spread = "S+3.00%", NetIrrPct = 7.1m, IrrTrend = "down", Moic = 0.94m, Status = "Non-accrual" },
        new() { Id = "deal-gale", Name = "Project Gale", Borrower = "Harborview REIT", Sector = "Real Estate", Country = "US", FundId = "fund-iii", Tranche = "Mezzanine", Invested = 70m, Outstanding = 66m, Spread = "S+4.00%", NetIrrPct = 18.4m, IrrTrend = "up", Moic = 1.27m, Status = "Performing" },
    ];

    // Mirrors src/mocks/investors.json (types, KYC states, wire-instruction flags).
    private static List<Investor> Investors() =>
    [
        new() { Id = "inv-redwood", Name = "Redwood Pension", Type = "Public Pension", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 40m, 28.4m), C("fund-ii", 18m, 17.3m)] },
        new() { Id = "inv-blueharbor", Name = "Blue Harbor Endowment", Type = "Endowment", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 25m, 17m)] },
        new() { Id = "inv-cascade", Name = "Cascade Family Office", Type = "Family Office", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 60m, 42.9m), C("fund-ii", 30m, 28.8m)] },
        new() { Id = "inv-granite", Name = "Granite State Insurance", Type = "Insurance", KycStatus = "In review", WireInstructionsOnFile = true, Commitments = [C("fund-ii", 35m, 33.6m)] },
        new() { Id = "inv-summit", Name = "Summit Investments", Type = "Fund of Funds", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 50m, 36m)] },
        new() { Id = "inv-oakmont", Name = "Oakmont Trust", Type = "Private Trust", WireInstructionsOnFile = false, Commitments = [C("fund-ii", 20m, 19.2m)] },
        new() { Id = "inv-ironwood", Name = "Ironwood Capital", Type = "Asset Manager", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 45m, 31.5m)] },
    ];

    private static InvestorCommitment C(string fundId, decimal amount, decimal called) =>
        new() { FundId = fundId, Amount = amount, Called = called };

    private static List<WorkflowStage> WorkflowStages() =>
    [
        S(1, "Operations", "Ops Analyst", 1), S(2, "Front Office", "Deal Lead", 1), S(3, "CIO", "CIO", 2),
        S(4, "Legal", "Counsel", 2), S(5, "Ops Final Review", "Ops Manager", 1), S(6, "Accounting", "Fund Accountant", 1),
        new() { Order = 7, Name = "Book", ApproverRole = "System", AutoAdvance = true, Required = true },
        new() { Order = 8, Name = "Custodians Notified", ApproverRole = "System", AutoAdvance = true },
        new() { Order = 9, Name = "Completed", ApproverRole = "", Terminal = true },
    ];

    private static WorkflowStage S(int order, string name, string role, int sla) =>
        new() { Order = order, Name = name, ApproverRole = role, SlaDays = sla, Required = true };

    private static List<EscalationRule> EscalationRules() =>
    [
        new()
        {
            Id = "esc-amount", Kind = EscalationRuleKind.AmountThreshold, Enabled = true,
            Condition = "Call amount > $20M", Effect = "require CIO + Compliance sign-off",
            ThresholdAmount = 20m, RequiredRoles = ["CIO", "Compliance"],
        },
        new()
        {
            Id = "esc-crossfund", Kind = EscalationRuleKind.CrossFundAllocation, Enabled = true,
            Condition = "Cross-fund allocation", Effect = "require Legal review", RequiredRoles = ["Counsel"],
        },
        new()
        {
            Id = "esc-newbank", Kind = EscalationRuleKind.NewBankAccount, Enabled = true,
            Condition = "Wire to a new bank account", Effect = "require dual authorization",
        },
    ];

    private static List<Role> Roles() =>
    [
        R("Ops Analyst", Capability.Edit, Capability.Approve, Capability.Edit, Capability.Edit, Capability.View, Capability.None),
        R("Deal Lead", Capability.Edit, Capability.Approve, Capability.View, Capability.None, Capability.View, Capability.None),
        R("CIO", Capability.View, Capability.Approve, Capability.View, Capability.View, Capability.View, Capability.None),
        R("Counsel", Capability.View, Capability.Approve, Capability.None, Capability.None, Capability.View, Capability.None),
        R("Fund Accountant", Capability.View, Capability.Approve, Capability.Edit, Capability.Edit, Capability.Edit, Capability.None),
        R("Compliance", Capability.View, Capability.Approve, Capability.View, Capability.View, Capability.View, Capability.View),
        R("Ops Manager", Capability.Edit, Capability.Approve, Capability.Edit, Capability.Edit, Capability.Edit, Capability.View),
        R("Administrator", Capability.Full, Capability.Full, Capability.Full, Capability.Full, Capability.Full, Capability.Full),
    ];

    private static Role R(string name, Capability blotter, Capability approvals, Capability wires,
        Capability recon, Capability refData, Capability admin) => new()
    {
        Name = name,
        Capabilities = new Dictionary<ModuleName, Capability>
        {
            [ModuleName.Blotter] = blotter, [ModuleName.Approvals] = approvals, [ModuleName.Wires] = wires,
            [ModuleName.Recon] = recon, [ModuleName.RefData] = refData, [ModuleName.Admin] = admin,
        },
    };

    private static List<StaffUser> StaffUsers() =>
    [
        U("u-jchen", "Jordan Chen", "JC", "jordan.chen@meridiancredit.com", "Ops Analyst"),
        U("u-mreyes", "Maria Reyes", "MR", "maria.reyes@meridiancredit.com", "Deal Lead"),
        U("u-spatel", "Sanjay Patel", "SP", "sanjay.patel@meridiancredit.com", "CIO"),
        U("u-jokafor", "J. Okafor", "JO", "j.okafor@meridiancredit.com", "Counsel"),
        U("u-dwhitfield", "Dana Whitfield", "DW", "dana.whitfield@meridiancredit.com", "Fund Accountant"),
        U("u-pnair", "Priya Nair", "PN", "priya.nair@meridiancredit.com", "Compliance"),
        U("u-talvarez", "Tom Alvarez", "TA", "tom.alvarez@meridiancredit.com", "Ops Manager"),
        U("u-akim", "alex.kim@meridiancredit.com", "AK", "alex.kim@meridiancredit.com", "Ops Analyst", "Invited"),
        U("u-admin", "Avery Whitman", "AW", "avery.whitman@meridiancredit.com", "Administrator"),
    ];

    private static StaffUser U(string id, string name, string initials, string email, string role, string status = "Active") =>
        new() { Id = id, Name = name, Initials = initials, Email = email, RoleName = role, Status = status };

    // Mirrors src/mocks/capital-calls.json exactly (ids, amounts, stages, allocations,
    // wire statuses, actors, dates). #C-2043/#C-2044 are over the $20M threshold and
    // carry the CIO + Compliance sign-offs the escalation rule injects (gate = CIO stage).
    private static List<CapitalCall> CapitalCalls() =>
    [
        new()
        {
            Id = "call-2036", Ref = "#C-2036", DealId = "deal-cedar", DealName = "Project Cedar", FundId = "fund-ii",
            Tranche = "Term B", Borrower = "Apex Manufacturing", Amount = 6.75m, DueDate = Day(7, 11),
            CurrentStage = 9, Status = CallStatus.Completed,
            Allocations = [A("inv-cascade", "Cascade Family Office", 30m, 6.75m, WireStatus.Confirmed)],
            StageEvents =
            [
                E(1, StageState.Done, "J. Chen", Day(6, 24)),
                E(2, StageState.Done, "M. Reyes", Day(6, 25)),
                E(3, StageState.Done, "S. Patel", Day(6, 25)),
                E(4, StageState.Done, "J. Okafor", Day(6, 26)),
                E(5, StageState.Done, "T. Alvarez", Day(6, 27)),
                E(6, StageState.Done, "D. Whitfield", Day(6, 28)),
                E(7, StageState.Done, "System", Day(6, 28)),
                E(8, StageState.Done, "System", Day(6, 28)),
                E(9, StageState.Done, "System", Day(6, 29)),
            ],
            Documents = [Doc("Capital Call Notice.pdf", "J. Chen", Day(6, 24))],
            AuditEntries =
            [
                Entry("Call created", "System", At(6, 24, 8, 0), "neutral"),
                Entry("Booked to GL", "System", At(6, 28, 16, 5), "green"),
                Entry("Custodians notified", "System", At(6, 28, 17, 20), "green"),
                Entry("Call completed", "System", At(6, 29, 8, 0), "green"),
            ],
        },
        new()
        {
            Id = "call-2039", Ref = "#C-2039", DealId = "deal-delta", DealName = "Project Delta", FundId = "fund-ii",
            Tranche = "Term A", Borrower = "Helio Software", Amount = 18.4m, DueDate = Day(7, 1),
            CurrentStage = 3, Status = CallStatus.Returned,
            Allocations =
            [
                A("inv-redwood", "Redwood Pension", 18m, 6.2m, WireStatus.Wired),
                A("inv-cascade", "Cascade Family Office", 30m, 5.1m, WireStatus.Wired),
                A("inv-granite", "Granite State Insurance", 35m, 4.3m, WireStatus.Overdue),
                A("inv-oakmont", "Oakmont Trust", 20m, 2.8m, WireStatus.Overdue),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "J. Chen", Day(6, 20)),
                E(2, StageState.Done, "M. Reyes", Day(6, 22)),
                new StageEvent { Stage = 3, State = StageState.Current, Actor = "S. Patel", Date = Day(7, 4), Note = "Returned for re-review" },
            ],
            Documents =
            [
                Doc("Capital Call Notice.pdf", "J. Chen", Day(6, 20)),
                Doc("Allocation Memo (rev 2).pdf", "D. Whitfield", Day(7, 4)),
            ],
            AuditEntries =
            [
                Entry("Call created", "System", At(6, 20, 8, 0), "neutral"),
                Entry("CIO approved", "S. Patel", At(6, 23, 12, 0), "green"),
                Entry("Allocation edited", "D. Whitfield", At(7, 4, 10, 15), "amber", "$5.10M → $5.00M correction reversed"),
                Entry("Returned to CIO", "J. Okafor", At(7, 4, 14, 22), "amber", "Allocation revision needs CIO re-approval"),
            ],
        },
        new()
        {
            Id = "call-2041", Ref = "#C-2041", DealId = "deal-atlas", DealName = "Project Atlas", FundId = "fund-iii",
            Tranche = "Term A", Borrower = "Vantage Health", Amount = 16m, DueDate = Day(7, 8),
            CurrentStage = 4, Status = CallStatus.InReview,
            Allocations =
            [
                A("inv-redwood", "Redwood Pension", 40m, 8.2m, WireStatus.Pending),
                A("inv-blueharbor", "Blue Harbor Endowment", 25m, 5.1m, WireStatus.Wired),
                A("inv-cascade", "Cascade Family Office", 60m, 2.7m, WireStatus.Scheduled),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "J. Chen", Day(7, 1)),
                E(2, StageState.Done, "M. Reyes", Day(7, 2)),
                E(3, StageState.Done, "S. Patel", Day(7, 2), comment: "Cleared to proceed"),
                new StageEvent { Stage = 4, State = StageState.Current, Actor = "J. Okafor", Date = Day(7, 4), Note = "In review" },
            ],
            Documents =
            [
                Doc("Capital Call Notice.pdf", "J. Chen", Day(7, 1)),
                Doc("Wire Instructions.pdf", "Operations", Day(7, 1)),
                Doc("LPA Excerpt — §4.2.pdf", "Legal", Day(7, 4)),
            ],
            AuditEntries =
            [
                Entry("Call created", "System", At(7, 1, 8, 0), "neutral"),
                Entry("Submitted for review", "J. Chen", At(7, 1, 14, 2), "green"),
                Entry("Front Office approved", "M. Reyes", At(7, 2, 9, 15), "green"),
                Entry("CIO approved", "S. Patel", At(7, 2, 16, 40), "green", "Cleared to proceed"),
                Entry("Legal review started", "J. Okafor", At(7, 4, 10, 12), "blue"),
            ],
        },
        new()
        {
            Id = "call-2042", Ref = "#C-2042", DealId = "deal-beacon", DealName = "Project Beacon", FundId = "fund-iii",
            Tranche = "Unitranche", Borrower = "Nordic Logistics", Amount = 26.3m, DueDate = Day(7, 9),
            CurrentStage = 5, Status = CallStatus.Pending,
            Allocations =
            [
                A("inv-cascade", "Cascade Family Office", 60m, 14.4m, WireStatus.Scheduled),
                A("inv-summit", "Summit Investments", 50m, 11.9m, WireStatus.Scheduled),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "J. Chen", Day(6, 30)),
                E(2, StageState.Done, "M. Reyes", Day(7, 1)),
                E(3, StageState.Done, "S. Patel", Day(7, 2)),
                E(4, StageState.Done, "J. Okafor", Day(7, 3)),
                new StageEvent { Stage = 5, State = StageState.Current, Actor = "T. Alvarez", Date = Day(7, 4), Note = "Awaiting review" },
            ],
            Documents =
            [
                Doc("Capital Call Notice.pdf", "J. Chen", Day(6, 30)),
                Doc("Wire Instructions.pdf", "Operations", Day(6, 30)),
            ],
            AuditEntries =
            [
                Entry("Call created", "System", At(6, 30, 8, 0), "neutral"),
                Entry("Submitted for review", "J. Chen", At(6, 30, 9, 12), "green"),
                Entry("Front Office approved", "M. Reyes", At(7, 1, 10, 30), "green"),
                Entry("CIO approved", "S. Patel", At(7, 2, 11, 5), "green"),
                Entry("Legal approved", "J. Okafor", At(7, 3, 15, 20), "green"),
            ],
        },
        new()
        {
            Id = "call-2043", Ref = "#C-2043", DealId = "deal-echo", DealName = "Project Echo", FundId = "fund-iii",
            Tranche = "Term A", Borrower = "Coastal Foods", Amount = 120m, DueDate = Day(8, 4),
            CurrentStage = 1, Status = CallStatus.InReview,
            EscalationSignoffs = AmountEscalation(),
            Allocations =
            [
                A("inv-redwood", "Redwood Pension", 40m, 21.8m, WireStatus.Pending),
                A("inv-blueharbor", "Blue Harbor Endowment", 25m, 13.6m, WireStatus.Pending),
                A("inv-cascade", "Cascade Family Office", 60m, 32.7m, WireStatus.Pending),
                A("inv-summit", "Summit Investments", 50m, 27.3m, WireStatus.Pending),
                A("inv-ironwood", "Ironwood Capital", 45m, 24.6m, WireStatus.Pending),
            ],
            StageEvents =
            [
                new StageEvent { Stage = 1, State = StageState.Current, Actor = "J. Chen", Date = Day(7, 4), Note = "In review · escalation: >$20M requires CIO + Compliance" },
            ],
            Documents = [Doc("Capital Call Notice (draft).pdf", "J. Chen", Day(7, 4))],
            AuditEntries = [Entry("Call created", "J. Chen", At(7, 4, 9, 30), "neutral")],
        },
        new()
        {
            Id = "call-2044", Ref = "#C-2044", DealId = "deal-gale", DealName = "Project Gale", FundId = "fund-iii",
            Tranche = "Mezzanine", Borrower = "Harborview REIT", Amount = 60.55m, DueDate = Day(7, 29),
            CurrentStage = 2, Status = CallStatus.InReview,
            EscalationSignoffs = AmountEscalation(),
            Allocations =
            [
                A("inv-redwood", "Redwood Pension", 40m, 11m, WireStatus.Pending),
                A("inv-blueharbor", "Blue Harbor Endowment", 25m, 6.9m, WireStatus.Pending),
                A("inv-cascade", "Cascade Family Office", 60m, 16.5m, WireStatus.Pending),
                A("inv-summit", "Summit Investments", 50m, 13.8m, WireStatus.Pending),
                A("inv-ironwood", "Ironwood Capital", 45m, 12.35m, WireStatus.Pending),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "J. Chen", Day(7, 2)),
                new StageEvent { Stage = 2, State = StageState.Current, Actor = "M. Reyes", Date = Day(7, 3), Note = "In review · escalation: >$20M requires CIO + Compliance" },
            ],
            Documents = [Doc("Capital Call Notice.pdf", "J. Chen", Day(7, 2))],
            AuditEntries =
            [
                Entry("Call created", "System", At(7, 2, 8, 0), "neutral"),
                Entry("Operations approved", "J. Chen", At(7, 2, 15, 45), "green"),
            ],
        },
    ];

    /// <summary>The esc-amount rule's injected sign-offs; gate = the CIO stage (3).</summary>
    private static List<EscalationSignoff> AmountEscalation() =>
    [
        new() { RuleId = "esc-amount", Role = "CIO", GateStage = 3 },
        new() { RuleId = "esc-amount", Role = "Compliance", GateStage = 3 },
    ];

    private static CallAllocation A(string id, string name, decimal commitment, decimal amount, WireStatus status) =>
        new() { InvestorId = id, InvestorName = name, Commitment = commitment, Amount = amount, WireStatus = status };

    private static StageEvent E(int stage, StageState state, string actor, DateOnly date, string? comment = null) =>
        new() { Stage = stage, State = state, Actor = actor, Date = date, Comment = comment };

    private static CallDocument Doc(string name, string by, DateOnly date) =>
        new() { Name = name, By = by, Date = date };

    private static CallAuditEntry Entry(string title, string by, DateTime at, string tone, string? comment = null) =>
        new() { Title = title, By = by, At = at, Tone = tone, Comment = comment };

    // Mirrors src/mocks/distributions.json exactly.
    private static List<Distribution> Distributions() =>
    [
        new()
        {
            Id = "dist-116", Ref = "#D-116", FundId = "fund-ii", Distributable = 10.6m, LpTotal = 10m, GpTotal = 0.6m,
            PaymentDate = Day(3, 31), Status = DistributionStatus.Paid, WaterfallType = WaterfallType.European,
            SourceNote = "Interest income",
            Tiers =
            [
                T("1 · Return of Capital", "Contributed capital", "100% LP", 7.5m, 7.5m, null, 3.1m),
                T("2 · Preferred Return", "Hurdle on capital", "8.0%", 1.7m, 1.7m, null, 1.4m),
                T("3 · GP Catch-up", "Until GP = 20% of profit", "100% GP", 0.4m, null, 0.4m, 1m),
                T("4 · Carried Interest", "Residual profit split", "80 / 20", 1m, 0.8m, 0.2m, 0m),
            ],
            Payouts =
            [
                P("inv-redwood", "Redwood Pension", 18m, 1.75m, 17.5m, PayoutStatus.Paid),
                P("inv-cascade", "Cascade Family Office", 30m, 2.91m, 29.1m, PayoutStatus.Paid),
                P("inv-granite", "Granite State Insurance", 35m, 3.4m, 34m, PayoutStatus.Paid),
                P("inv-oakmont", "Oakmont Trust", 20m, 1.94m, 19.4m, PayoutStatus.Paid),
            ],
        },
        new()
        {
            Id = "dist-117", Ref = "#D-117", FundId = "fund-iii", Distributable = 19.9m, LpTotal = 18.7m, GpTotal = 1.2m,
            PaymentDate = Day(6, 30), Status = DistributionStatus.Paid, WaterfallType = WaterfallType.European,
            SourceNote = "Interest income",
            Tiers =
            [
                T("1 · Return of Capital", "Contributed capital", "100% LP", 14m, 14m, null, 5.9m),
                T("2 · Preferred Return", "Hurdle on capital", "8.0%", 2.9m, 2.9m, null, 3m),
                T("3 · GP Catch-up", "Until GP = 20% of profit", "100% GP", 0.75m, null, 0.75m, 2.25m),
                T("4 · Carried Interest", "Residual profit split", "80 / 20", 2.25m, 1.8m, 0.45m, 0m),
            ],
            Payouts =
            [
                P("inv-redwood", "Redwood Pension", 40m, 3.4m, 18.2m, PayoutStatus.Paid),
                P("inv-blueharbor", "Blue Harbor Endowment", 25m, 2.13m, 11.4m, PayoutStatus.Paid),
                P("inv-cascade", "Cascade Family Office", 60m, 5.1m, 27.3m, PayoutStatus.Paid),
                P("inv-summit", "Summit Investments", 50m, 4.25m, 22.7m, PayoutStatus.Paid),
                P("inv-ironwood", "Ironwood Capital", 45m, 3.82m, 20.4m, PayoutStatus.Paid),
            ],
        },
        new()
        {
            Id = "dist-118", Ref = "#D-118", FundId = "fund-iii", Distributable = 42m, LpTotal = 39.2m, GpTotal = 2.8m,
            PaymentDate = Day(9, 30), Status = DistributionStatus.Scheduled, WaterfallType = WaterfallType.European,
            SourceNote = "Loan repayments + interest",
            Tiers =
            [
                T("1 · Return of Capital", "Contributed capital", "100% LP", 28m, 28m, null, 14m),
                T("2 · Preferred Return", "Hurdle on capital", "8.0%", 6m, 6m, null, 8m),
                T("3 · GP Catch-up", "Until GP = 20% of profit", "100% GP", 1.5m, null, 1.5m, 6.5m),
                T("4 · Carried Interest", "Residual profit split", "80 / 20", 6.5m, 5.2m, 1.3m, 0m),
            ],
            Payouts =
            [
                P("inv-redwood", "Redwood Pension", 40m, 7.13m, 18.2m, PayoutStatus.Scheduled),
                P("inv-blueharbor", "Blue Harbor Endowment", 25m, 4.45m, 11.4m, PayoutStatus.Scheduled),
                P("inv-cascade", "Cascade Family Office", 60m, 10.69m, 27.3m, PayoutStatus.Scheduled),
                P("inv-summit", "Summit Investments", 50m, 8.91m, 22.7m, PayoutStatus.Scheduled),
                P("inv-ironwood", "Ironwood Capital", 45m, 8.02m, 20.4m, PayoutStatus.Scheduled),
            ],
        },
        new()
        {
            Id = "dist-119", Ref = "#D-119", FundId = "fund-ii", Distributable = 12.75m, LpTotal = 12m, GpTotal = 0.75m,
            PaymentDate = Day(7, 3), Status = DistributionStatus.Paying, WaterfallType = WaterfallType.European,
            SourceNote = "Cedar repayment + interest",
            Tiers =
            [
                T("1 · Return of Capital", "Contributed capital", "100% LP", 9m, 9m, null, 3.75m),
                T("2 · Preferred Return", "Hurdle on capital", "8.0%", 2m, 2m, null, 1.75m),
                T("3 · GP Catch-up", "Until GP = 20% of profit", "100% GP", 0.5m, null, 0.5m, 1.25m),
                T("4 · Carried Interest", "Residual profit split", "80 / 20", 1.25m, 1m, 0.25m, 0m),
            ],
            Payouts =
            [
                P("inv-redwood", "Redwood Pension", 18m, 2.1m, 17.5m, PayoutStatus.Sent, wireRef: "W-8843"),
                P("inv-cascade", "Cascade Family Office", 30m, 3.5m, 29.2m, PayoutStatus.Paid, wireRef: "W-8844"),
                P("inv-granite", "Granite State Insurance", 35m, 4.08m, 34m, PayoutStatus.Exception, wireRef: "W-8847"),
                P("inv-oakmont", "Oakmont Trust", 20m, 2.32m, 19.3m, PayoutStatus.Blocked, blockedReason: "No wire instructions on file"),
            ],
        },
    ];

    private static WaterfallTier T(string tier, string basis, string rate, decimal distributed,
        decimal? lp, decimal? gp, decimal poolLeft) => new()
    {
        Tier = tier, Basis = basis, Rate = rate, Distributed = distributed, LpShare = lp, GpShare = gp, PoolLeft = poolLeft,
    };

    private static InvestorPayout P(string id, string name, decimal commitment, decimal amount, decimal pct,
        PayoutStatus status, string? blockedReason = null, string? wireRef = null) => new()
    {
        InvestorId = id, InvestorName = name, Commitment = commitment, Amount = amount, PctOfLpTotal = pct,
        Status = status, BlockedReason = blockedReason, WireRef = wireRef,
    };

    private static List<AuditEvent> AuditEvents()
    {
        var events = new List<AuditEvent>();
        string? previous = null;

        void Append(DateTime at, string actor, string action, string tone, string subject, string detail)
        {
            var seal = AuditSealer.ComputeSeal(previous, at, actor, action, subject, detail);
            events.Add(new AuditEvent { At = at, Actor = actor, Action = action, Tone = tone, Subject = subject, Detail = detail, Seal = seal });
            previous = seal;
        }

        Append(At(7, 1, 8, 0), "System", "Created", "neutral", "Call #C-2041 · Project Atlas", "$16.00M due 2026-07-08 · basis unfunded");
        Append(At(7, 2, 9, 15), "Maria Reyes", "Approved", "green", "Call #C-2041 · Front Office", "");
        Append(At(7, 2, 16, 40), "Sanjay Patel", "Approved", "green", "Call #C-2041 · CIO", "“Cleared to proceed”");
        Append(At(7, 3, 9, 0), "System", "Payment run", "blue", "Distribution #D-119", "$12.00M LP payouts queued");
        Append(At(7, 3, 11, 20), "System", "Exception", "red", "Wire W-8847 · Granite State Insurance", "SWIFT gateway rejected — certificate expired");
        Append(At(7, 4, 14, 22), "J. Okafor", "Returned", "amber", "Call #C-2039 · returned to CIO", "“Allocation revision needs CIO re-approval”");

        return events;
    }
}
