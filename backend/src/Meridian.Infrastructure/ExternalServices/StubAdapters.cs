using Meridian.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Meridian.Infrastructure.ExternalServices;

// Local stand-ins for the external integrations. Each implements an Application
// port, so swapping in the real SWIFT/Fedwire gateway, custodian SFTP/API feed or
// blob storage is a DI registration change — nothing above this layer moves.

public sealed class NoopWireGateway(ILogger<NoopWireGateway> logger) : IWireGateway
{
    public Task<WireSubmissionResult> SubmitAsync(WireInstruction instruction, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Wire submitted to {Rail} for {Counterparty}: {Reference}",
            instruction.Rail, instruction.Counterparty, instruction.Reference);
        return Task.FromResult(new WireSubmissionResult(true, null));
    }
}

public sealed class StaticCustodianFeed(IClock clock) : ICustodianFeed
{
    public Task<CustodianSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new CustodianSnapshot(clock.UtcNow, RecordCount: 128));
}

public sealed class LocalDocumentStorage : IDocumentStorage
{
    public Task<Uri> GetSignedUrlAsync(string documentName, TimeSpan timeToLive, CancellationToken cancellationToken = default) =>
        Task.FromResult(new Uri(
            $"https://localhost/documents/{Uri.EscapeDataString(documentName)}?expires={(int)timeToLive.TotalSeconds}s"));
}
