namespace Meridian.Domain.Entities;

public class InvestorCommitment
{
    public string FundId { get; set; } = "";
    public decimal Amount { get; set; }
    /// <summary>Amount called to date; the wizard's default basis is <see cref="Unfunded"/>.</summary>
    public decimal Called { get; set; }
    public decimal Unfunded => Amount - Called;
}

public class Investor
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string KycStatus { get; set; } = "Verified";
    /// <summary>Gates payouts and wires — no wire may ever be created without instructions on file.</summary>
    public bool WireInstructionsOnFile { get; set; }
    public List<InvestorCommitment> Commitments { get; set; } = [];
}
