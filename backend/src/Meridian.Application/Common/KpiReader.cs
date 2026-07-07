using Meridian.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Common;

/// <summary>The published KPI metrics of one screen, keyed by metric name.</summary>
public sealed class KpiBag(IReadOnlyDictionary<string, (decimal? Number, string? Text)> metrics)
{
    public decimal Number(string metric) => metrics.TryGetValue(metric, out var value) ? value.Number ?? 0m : 0m;

    public int Count(string metric) => (int)Number(metric);

    public string Text(string metric) => metrics.TryGetValue(metric, out var value) ? value.Text ?? "" : "";
}

public static class KpiReader
{
    public static async Task<KpiBag> ForScreenAsync(IAppDbContext db, string screenKey, CancellationToken ct)
    {
        var metrics = await db.KpiSnapshots.AsNoTracking()
            .Where(k => k.ScreenKey == screenKey)
            .ToDictionaryAsync(k => k.MetricKey, k => (k.NumericValue, k.TextValue), ct);
        return new KpiBag(metrics);
    }
}
