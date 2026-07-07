namespace Meridian.Application.Abstractions;

// Ports for external integrations. Everything that leaves the process goes through
// one of these interfaces so vendors (SWIFT/Fedwire rails, custodian feeds, document
// stores) stay swappable and the core remains testable without network access.

public sealed record WireInstruction(string Rail, string Counterparty, decimal AmountUsdMillions, string Reference);

public sealed record WireSubmissionResult(bool Accepted, string? Reason);

/// <summary>Payment-rail gateway (Fedwire / ACH / SWIFT).</summary>
public interface IWireGateway
{
    Task<WireSubmissionResult> SubmitAsync(WireInstruction instruction, CancellationToken cancellationToken = default);
}

public sealed record CustodianSnapshot(DateTime AsOf, int RecordCount);

/// <summary>Custodian position/cash feed consumed by the reconciliation engine.</summary>
public interface ICustodianFeed
{
    Task<CustodianSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default);
}

/// <summary>Signed-URL document delivery, gated by investor-access document-type config.</summary>
public interface IDocumentStorage
{
    Task<Uri> GetSignedUrlAsync(string documentName, TimeSpan timeToLive, CancellationToken cancellationToken = default);
}
