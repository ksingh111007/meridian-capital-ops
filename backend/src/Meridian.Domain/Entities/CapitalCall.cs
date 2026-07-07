namespace Meridian.Domain.Entities;

public class CallAllocation
{
    public string InvestorId { get; set; } = "";
    public string InvestorName { get; set; } = "";
    /// <summary>The LP's commitment in the call's fund.</summary>
    public decimal Commitment { get; set; }
    public decimal Amount { get; set; }
    public WireStatus WireStatus { get; set; } = WireStatus.Pending;
}

public class StageEvent
{
    public int Stage { get; set; }
    public StageState State { get; set; }
    public string? Actor { get; set; }
    public DateOnly? Date { get; set; }
    public string? Note { get; set; }
    public string? Comment { get; set; }
}

public class CallDocument
{
    public string Name { get; set; } = "";
    public string By { get; set; } = "";
    public DateOnly Date { get; set; }
}

public class CallAuditEntry
{
    public string Title { get; set; } = "";
    public string By { get; set; } = "";
    public DateTime At { get; set; }
    public string? Comment { get; set; }
    public string Tone { get; set; } = "neutral";
}

/// <summary>
/// An outstanding sign-off injected by an escalation rule. Each carries its own
/// gate so rules with different stages never weaken each other: the call cannot
/// advance from any stage at or past <see cref="GateStage"/> while pending.
/// </summary>
public class EscalationSignoff
{
    public string RuleId { get; set; } = "";
    public string Role { get; set; } = "";
    public int GateStage { get; set; }
}

public class CapitalCall
{
    public string Id { get; set; } = "";
    public string Ref { get; set; } = "";
    public string DealId { get; set; } = "";
    public string DealName { get; set; } = "";
    public string FundId { get; set; } = "";
    public string Tranche { get; set; } = "";
    public string Borrower { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    /// <summary>Recorded per BUSINESS_RULES.md — which basis the allocations defaulted from.</summary>
    public AllocationBasis Basis { get; set; } = AllocationBasis.Unfunded;
    /// <summary>1-based against the workflow's ordered stages; the terminal stage means Completed.</summary>
    public int CurrentStage { get; set; } = 1;
    public CallStatus Status { get; set; } = CallStatus.InReview;
    /// <summary>
    /// Optimistic-concurrency token — services bump it on every workflow mutation so
    /// concurrent approvals/rejections of the same call conflict instead of both committing.
    /// </summary>
    public int Version { get; set; }
    /// <summary>Outstanding escalation sign-offs (empty once all have landed).</summary>
    public List<EscalationSignoff> EscalationSignoffs { get; set; } = [];
    public List<CallAllocation> Allocations { get; set; } = [];
    public List<StageEvent> StageEvents { get; set; } = [];
    public List<CallDocument> Documents { get; set; } = [];
    public List<CallAuditEntry> AuditEntries { get; set; } = [];
}
