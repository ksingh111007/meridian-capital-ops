using Meridian.Application.Abstractions;

namespace Meridian.Application.Automation;

/// <summary>
/// Scaffold for the recon feed sync: pulls the latest custodian snapshot through
/// the <see cref="ICustodianFeed"/> port and records the sync. The auto-match
/// engine plugs in here when the reconciliation module is built.
/// </summary>
public class CustodianSyncService(ICustodianFeed feed, IAuditTrail audit)
{
    public async Task<int> SyncAsync(CancellationToken ct = default)
    {
        var snapshot = await feed.FetchLatestAsync(ct);
        await audit.AppendAsync("System", "Feed sync", "neutral", "Custodian feed",
            $"{snapshot.RecordCount} records as of {snapshot.AsOf:yyyy-MM-dd HH:mm}", ct);
        return snapshot.RecordCount;
    }
}
