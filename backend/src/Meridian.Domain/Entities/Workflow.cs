namespace Meridian.Domain.Entities;

public class WorkflowStage
{
    /// <summary>1-based order; also the primary key.</summary>
    public int Order { get; set; }
    public string Name { get; set; } = "";
    /// <summary>"System" for automated stages, "" for the terminal stage.</summary>
    public string ApproverRole { get; set; } = "";
    public int? SlaDays { get; set; }
    public bool AutoAdvance { get; set; }
    public bool Required { get; set; }
    public bool Terminal { get; set; }
}

public class EscalationRule
{
    public string Id { get; set; } = "";
    public EscalationRuleKind Kind { get; set; }
    /// <summary>Display string shown in admin, e.g. "Call amount &gt; $20M".</summary>
    public string Condition { get; set; } = "";
    public string Effect { get; set; } = "";
    public bool Enabled { get; set; }
    /// <summary>For <see cref="EscalationRuleKind.AmountThreshold"/>, in USD millions.</summary>
    public decimal? ThresholdAmount { get; set; }
    /// <summary>Roles whose sign-off the rule injects.</summary>
    public List<string> RequiredRoles { get; set; } = [];
}
