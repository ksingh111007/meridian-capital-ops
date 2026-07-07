using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.FundOps;

public sealed record ReconKpisDto(
    decimal MatchedPct, int MatchedCount, int TotalItems, int Exceptions, int Unmatched, decimal AmountInBreak);

public sealed record ReconItemDto(
    string Id, string Date, string Description, string Source, decimal? Book, decimal? Custodian,
    decimal Diff, string Status, string? Assignee);

public sealed record ReconciliationDto(string AsOf, string Source, ReconKpisDto Kpis, IReadOnlyList<ReconItemDto> Items);

public class ReconciliationService(IAppDbContext db, IAuditTrail audit, ICurrentUserProvider currentUser)
{
    public async Task<ReconciliationDto> GetAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "reconciliation", ct);
        var items = await db.ReconItems.AsNoTracking().OrderBy(i => i.Id).ToListAsync(ct);

        return new ReconciliationDto(
            kpis.Text("asOf"), kpis.Text("source"),
            new ReconKpisDto(
                kpis.Number("matchedPct"), kpis.Count("matchedCount"), kpis.Count("totalItems"),
                kpis.Count("exceptions"), kpis.Count("unmatched"), kpis.Number("amountInBreak")),
            items.Select(ToDto).ToList());
    }

    /// <summary>Assigns a Break/Unmatched item to an owner; audited like every mutation.</summary>
    public async Task<ReconItemDto> AssignAsync(string id, string assignee, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(assignee))
            throw DomainException.Validation("An assignee is required.");

        var user = await currentUser.GetRequiredAsync(ct);
        var item = await db.ReconItems.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw DomainException.NotFound($"Reconciliation item '{id}' was not found.");

        if (item.Status == "Matched")
            throw DomainException.Conflict("Matched items cannot be assigned — only Breaks and Unmatched items.");

        item.Assignee = assignee.Trim();
        await audit.AppendAsync(user.Name, "Recon assigned", "blue",
            $"Recon {item.Id} · {item.Description}",
            $"{Display.Money(item.Diff)} {item.Status} → {item.Assignee}", ct);

        return ToDto(item);
    }

    private static ReconItemDto ToDto(Domain.Entities.ReconItem i) => new(
        i.Id, i.Date.ToString("yyyy-MM-dd"), i.Description, i.Source, i.Book, i.Custodian, i.Diff, i.Status, i.Assignee);
}
