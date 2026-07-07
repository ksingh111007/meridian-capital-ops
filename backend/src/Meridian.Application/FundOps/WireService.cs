using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.FundOps;

public sealed record WireKpisDto(
    int WiresToday, decimal SettledAmount, int SettledCount, decimal InFlightAmount, int InFlightCount, int Exceptions);

public sealed record WireDto(
    string Id, string Ref, string Direction, string Counterparty, string Type, string LinkedRef,
    decimal Amount, string Time, string Date, string Rail, string Status, string? ExceptionReason);

public sealed record WiresDto(string AsOf, WireKpisDto Kpis, IReadOnlyList<WireDto> Wires);

public class WireService(
    IAppDbContext db, IWireGateway wireGateway, IAuditTrail audit, ICurrentUserProvider currentUser, IClock clock)
{
    public async Task<WiresDto> GetAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "wires", ct);
        var wires = await db.Wires.AsNoTracking().OrderBy(w => w.Ref).ToListAsync(ct);

        // KPIs are computed from the rows they summarize, so mutations (retry)
        // can never leave the strip contradicting the list below it.
        var today = wires.Where(w => w.Date == clock.Today).ToList();
        var settled = today.Where(w => w.Status == "Settled").ToList();
        var inFlight = today.Where(w => w.Status is "Queued" or "Sent" or "Acknowledged").ToList();

        return new WiresDto(
            kpis.Text("asOf"),
            new WireKpisDto(
                today.Count, settled.Sum(w => w.Amount), settled.Count,
                inFlight.Sum(w => w.Amount), inFlight.Count,
                wires.Count(w => w.Status == "Exception")),
            wires.Select(ToDto).ToList());
    }

    /// <summary>
    /// Re-queues an Exception wire. Re-checks wire instructions on file for LP
    /// counterparties (no wire may ever exist without them) and audits the retry.
    /// </summary>
    public async Task<WireDto> RetryAsync(string id, CancellationToken ct = default)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        var wire = await db.Wires.FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw DomainException.NotFound($"Wire '{id}' was not found.");

        if (wire.Status != "Exception")
            throw DomainException.Conflict($"Wire {wire.Ref} is {wire.Status}; only Exception wires can be retried.");

        // LP-facing wire types must fail closed: an unknown counterparty means the
        // instructions check cannot run, not that it passed (BUSINESS_RULES — no
        // wire to an LP without instructions on file). Facility wires go to banks.
        if (wire.Type is "Capital Call" or "Distribution")
        {
            var counterparty = await db.Investors.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Name == wire.Counterparty, ct);
            if (counterparty is null)
                throw DomainException.Conflict(
                    $"'{wire.Counterparty}' does not match a registered investor — verify the counterparty before retrying.");
            if (!counterparty.WireInstructionsOnFile)
                throw DomainException.Validation($"{wire.Counterparty} has no wire instructions on file.");
        }

        var submission = await wireGateway.SubmitAsync(
            new WireInstruction(wire.Rail, wire.Counterparty, wire.Amount, wire.Ref), ct);
        if (!submission.Accepted)
            throw DomainException.Conflict($"The payment gateway rejected the retry: {submission.Reason}");

        wire.Status = "Queued";
        wire.ExceptionReason = null;
        await audit.AppendAsync(user.Name, "Wire retried", "blue",
            $"Wire {wire.Ref} · {wire.Counterparty}",
            $"{Display.Money(wire.Amount)} re-queued via {wire.Rail}", ct);

        return ToDto(wire);
    }

    private static WireDto ToDto(Domain.Entities.Wire w) => new(
        w.Id, w.Ref, w.Direction, w.Counterparty, w.Type, w.LinkedRef, w.Amount, w.Time,
        w.Date.ToString("yyyy-MM-dd"), w.Rail, w.Status, w.ExceptionReason);
}
