using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Seeding;

/// <summary>
/// Machine-side seed: entity skeletons (ids, commitments) plus the workflow,
/// escalation-rule, role, and staff configuration the engine needs. The story
/// values themselves (deals, funds, calls, distributions, audit log, KPI strips)
/// come from the frontend mock JSONs via <see cref="MockDataSeed"/> — the same
/// files that generate the Azure SQL seed — so every store serves one story.
/// Business "today" is pinned to 2026-07-05 in dev.
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
    }

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
}
