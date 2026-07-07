using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Seeding;

/// <summary>
/// Deterministic sample data mirroring the frontend mock story
/// (meridian-capital-ops/src/mocks + docs/DATA_MODEL.md § Cross-screen consistency):
/// #C-2041 sits at Legal awaiting J. Okafor, #C-2039 is Returned at CIO with two
/// overdue wires, #D-119 has Oakmont Blocked (no wire instructions) and Granite in
/// Exception. Business "today" is pinned to 2026-07-05 in dev. Illustrative, not
/// authoritative — replaced by the real database project later.
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

    private static List<Deal> Deals() =>
    [
        new() { Id = "deal-atlas", Name = "Project Atlas", Borrower = "Vantage Health", Sector = "Healthcare", Country = "US", FundId = "fund-iii", Tranche = "Term A", Invested = 85m, Outstanding = 80m, Spread = "S+2.50%", NetIrrPct = 13.8m, IrrTrend = "up", Moic = 1.12m, Status = "Performing" },
        new() { Id = "deal-beacon", Name = "Project Beacon", Borrower = "Beacon Software", Sector = "Technology", Country = "US", FundId = "fund-iii", Tranche = "Unitranche", Invested = 60m, Outstanding = 58m, Spread = "S+3.25%", NetIrrPct = 12.4m, IrrTrend = "flat", Moic = 1.08m, Status = "Performing" },
        new() { Id = "deal-delta", Name = "Project Delta", Borrower = "Delta Logistics", Sector = "Transportation", Country = "US", FundId = "fund-ii", Tranche = "Term B", Invested = 45m, Outstanding = 44m, Spread = "S+4.00%", NetIrrPct = 9.1m, IrrTrend = "down", Moic = 1.21m, Status = "Watch" },
        new() { Id = "deal-cedar", Name = "Project Cedar", Borrower = "Cedar Industrial", Sector = "Industrials", Country = "US", FundId = "fund-ii", Tranche = "Term A", Invested = 38m, Outstanding = 0m, Spread = "S+2.75%", NetIrrPct = 11.6m, IrrTrend = "up", Moic = 1.34m, Status = "Performing" },
    ];

    private static List<Investor> Investors() =>
    [
        new() { Id = "inv-redwood", Name = "Redwood Pension", Type = "Public pension", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 40m, 28.4m), C("fund-ii", 18m, 17.3m)] },
        new() { Id = "inv-blueharbor", Name = "Blue Harbor Endowment", Type = "Endowment", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 25m, 17m)] },
        new() { Id = "inv-cascade", Name = "Cascade Family Office", Type = "Family office", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 60m, 42.9m), C("fund-ii", 30m, 28.8m)] },
        new() { Id = "inv-granite", Name = "Granite State Insurance", Type = "Insurance", WireInstructionsOnFile = true, Commitments = [C("fund-ii", 35m, 33.6m)] },
        new() { Id = "inv-summit", Name = "Summit Investments", Type = "Asset manager", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 50m, 36m)] },
        new() { Id = "inv-oakmont", Name = "Oakmont Trust", Type = "Trust", WireInstructionsOnFile = false, Commitments = [C("fund-ii", 20m, 19.2m)] },
        new() { Id = "inv-ironwood", Name = "Ironwood Capital", Type = "Fund of funds", WireInstructionsOnFile = true, Commitments = [C("fund-iii", 45m, 31.5m)] },
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

    private static List<CapitalCall> CapitalCalls() =>
    [
        new()
        {
            Id = "call-2036", Ref = "#C-2036", DealId = "deal-cedar", DealName = "Project Cedar", FundId = "fund-ii",
            Tranche = "Term A", Borrower = "Cedar Industrial", Amount = 6.75m, DueDate = Day(6, 15),
            CurrentStage = 9, Status = CallStatus.Completed,
            Allocations =
            [
                A("inv-granite", "Granite State Insurance", 35m, 3.5m, WireStatus.Confirmed),
                A("inv-oakmont", "Oakmont Trust", 20m, 3.25m, WireStatus.Confirmed),
            ],
            StageEvents = Enumerable.Range(1, 8).Select(order => new StageEvent
            {
                Stage = order, State = StageState.Done,
                Actor = order >= 7 ? "System" : "—", Date = Day(6, 8 + order),
            }).ToList(),
            Documents = [new CallDocument { Name = "Capital Call Notice.pdf", By = "Jordan Chen", Date = Day(6, 8) }],
            AuditEntries =
            [
                new CallAuditEntry { Title = "Call created", By = "Jordan Chen", At = At(6, 8, 9, 0), Tone = "neutral" },
                new CallAuditEntry { Title = "Completed — custodians notified", By = "System", At = At(6, 16, 17, 5), Tone = "green" },
            ],
        },
        new()
        {
            Id = "call-2039", Ref = "#C-2039", DealId = "deal-delta", DealName = "Project Delta", FundId = "fund-ii",
            Tranche = "Term B", Borrower = "Delta Logistics", Amount = 18.4m, DueDate = Day(7, 1),
            CurrentStage = 3, Status = CallStatus.Returned,
            Allocations =
            [
                A("inv-granite", "Granite State Insurance", 35m, 4.3m, WireStatus.Overdue),
                A("inv-oakmont", "Oakmont Trust", 20m, 2.8m, WireStatus.Overdue),
                A("inv-cascade", "Cascade Family Office", 30m, 6.5m, WireStatus.Wired),
                A("inv-redwood", "Redwood Pension", 18m, 4.8m, WireStatus.Confirmed),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "Jordan Chen", Day(6, 24)),
                E(2, StageState.Done, "Maria Reyes", Day(6, 25)),
                new StageEvent { Stage = 3, State = StageState.Current, Date = Day(7, 4), Note = "Returned from Legal" },
            ],
            Documents = [new CallDocument { Name = "Capital Call Notice.pdf", By = "Jordan Chen", Date = Day(6, 24) }],
            AuditEntries =
            [
                new CallAuditEntry { Title = "Call created", By = "Jordan Chen", At = At(6, 24, 10, 0), Tone = "neutral" },
                new CallAuditEntry { Title = "Legal returned the call to CIO", By = "J. Okafor", At = At(7, 4, 15, 30), Comment = "Allocations edited after approval began — re-approval required", Tone = "amber" },
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
                E(1, StageState.Done, "Jordan Chen", Day(7, 1)),
                E(2, StageState.Done, "Maria Reyes", Day(7, 2)),
                E(3, StageState.Done, "Sanjay Patel", Day(7, 2), comment: "Cleared to proceed"),
                new StageEvent { Stage = 4, State = StageState.Current, Actor = "J. Okafor", Date = Day(7, 4), Note = "In review" },
            ],
            Documents =
            [
                new CallDocument { Name = "Capital Call Notice.pdf", By = "Jordan Chen", Date = Day(7, 1) },
                new CallDocument { Name = "Wire Instructions.pdf", By = "Operations", Date = Day(7, 1) },
                new CallDocument { Name = "LPA Excerpt — §4.2.pdf", By = "Legal", Date = Day(7, 4) },
            ],
            AuditEntries =
            [
                new CallAuditEntry { Title = "Call created", By = "System", At = At(7, 1, 8, 0), Tone = "neutral" },
                new CallAuditEntry { Title = "Submitted for review", By = "Jordan Chen", At = At(7, 1, 14, 2), Tone = "green" },
                new CallAuditEntry { Title = "Front Office approved", By = "Maria Reyes", At = At(7, 2, 9, 15), Tone = "green" },
                new CallAuditEntry { Title = "CIO approved", By = "Sanjay Patel", At = At(7, 2, 16, 40), Comment = "Cleared to proceed", Tone = "green" },
                new CallAuditEntry { Title = "Legal review started", By = "J. Okafor", At = At(7, 4, 10, 12), Tone = "blue" },
            ],
        },
        new()
        {
            Id = "call-2042", Ref = "#C-2042", DealId = "deal-beacon", DealName = "Project Beacon", FundId = "fund-iii",
            Tranche = "Unitranche", Borrower = "Beacon Software", Amount = 26.3m, DueDate = Day(7, 15),
            CurrentStage = 5, Status = CallStatus.Pending,
            Allocations =
            [
                A("inv-summit", "Summit Investments", 50m, 10.5m, WireStatus.Pending),
                A("inv-ironwood", "Ironwood Capital", 45m, 9.4m, WireStatus.Pending),
                A("inv-cascade", "Cascade Family Office", 60m, 6.4m, WireStatus.Pending),
            ],
            StageEvents =
            [
                E(1, StageState.Done, "Jordan Chen", Day(6, 30)),
                E(2, StageState.Done, "Maria Reyes", Day(7, 1)),
                E(3, StageState.Done, "Sanjay Patel", Day(7, 2)),
                E(4, StageState.Done, "J. Okafor", Day(7, 3)),
                new StageEvent { Stage = 5, State = StageState.Current, Date = Day(7, 3), Note = "In review" },
            ],
            Documents = [new CallDocument { Name = "Capital Call Notice.pdf", By = "Jordan Chen", Date = Day(6, 30) }],
            AuditEntries =
            [
                new CallAuditEntry { Title = "Call created", By = "Jordan Chen", At = At(6, 30, 9, 30), Tone = "neutral" },
                new CallAuditEntry { Title = "Escalation cleared — CIO + Compliance sign-off", By = "System", At = At(7, 2, 12, 0), Tone = "green" },
            ],
        },
    ];

    private static CallAllocation A(string id, string name, decimal commitment, decimal amount, WireStatus status) =>
        new() { InvestorId = id, InvestorName = name, Commitment = commitment, Amount = amount, WireStatus = status };

    private static StageEvent E(int stage, StageState state, string actor, DateOnly date, string? comment = null) =>
        new() { Stage = stage, State = state, Actor = actor, Date = date, Comment = comment };

    private static List<Distribution> Distributions() =>
    [
        new()
        {
            Id = "dist-118", Ref = "#D-118", FundId = "fund-iii", Distributable = 8m, LpTotal = 7.4m, GpTotal = 0.6m,
            PaymentDate = Day(6, 20), Status = DistributionStatus.Paid, WaterfallType = WaterfallType.European,
            SourceNote = "Beacon interest + amortization",
            Tiers =
            [
                T("1 · Return of Capital", "Contributed capital", "100% LP", 5m, 5m, null, 3m),
                T("2 · Preferred Return", "Hurdle on capital", "8.0%", 1.2m, 1.2m, null, 1.8m),
                T("3 · GP Catch-up", "Until GP = 20% of profit", "100% GP", 0.3m, null, 0.3m, 1.5m),
                T("4 · Carried Interest", "Residual profit split", "80 / 20", 1.5m, 1.2m, 0.3m, 0m),
            ],
            Payouts =
            [
                P("inv-redwood", "Redwood Pension", 40m, 1.35m, 18.2m, PayoutStatus.Paid, wireRef: "W-8830"),
                P("inv-blueharbor", "Blue Harbor Endowment", 25m, 0.84m, 11.4m, PayoutStatus.Paid, wireRef: "W-8831"),
                P("inv-cascade", "Cascade Family Office", 60m, 2.02m, 27.3m, PayoutStatus.Paid, wireRef: "W-8832"),
                P("inv-summit", "Summit Investments", 50m, 1.68m, 22.7m, PayoutStatus.Paid, wireRef: "W-8833"),
                P("inv-ironwood", "Ironwood Capital", 45m, 1.51m, 20.4m, PayoutStatus.Paid, wireRef: "W-8834"),
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
        Append(At(7, 4, 15, 30), "J. Okafor", "Returned", "amber", "Call #C-2039 · returned to CIO", "“Allocations edited after approval began — re-approval required”");

        return events;
    }
}
