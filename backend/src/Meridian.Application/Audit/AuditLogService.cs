using System.Text.Json.Serialization;
using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Audit;

public sealed record AuditEventDto(
    string Time,
    string AtUtc,
    string Actor,
    string Action,
    string Tone,
    [property: JsonPropertyName("object")] string Subject,
    string Detail,
    string Seal);

public sealed record AuditKpisDto(int EventsToday, int Approvals, int ConfigChanges, int Exceptions, bool ChainValid);

public sealed record AuditLogDto(AuditKpisDto Kpis, IReadOnlyList<AuditEventDto> Events);

public class AuditLogService(IAppDbContext db, IClock clock)
{
    public async Task<AuditLogDto> GetAsync(CancellationToken ct = default)
    {
        var events = await db.AuditEvents.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);

        var kpis = new AuditKpisDto(
            EventsToday: events.Count(e => DateOnly.FromDateTime(e.At) == clock.Today),
            Approvals: events.Count(e => e.Action is "Approved" or "Escalation sign-off"),
            ConfigChanges: events.Count(e => e.Action == "Config"),
            Exceptions: events.Count(e => e.Tone == "red"),
            ChainValid: AuditSealer.VerifyChain(events));

        var rows = events
            .OrderByDescending(e => e.Id)
            .Select(e => new AuditEventDto(
                Display.ShortDateTime(e.At), e.At.ToString("O"), e.Actor, e.Action, e.Tone, e.Subject, e.Detail, e.Seal))
            .ToList();

        return new AuditLogDto(kpis, rows);
    }
}
