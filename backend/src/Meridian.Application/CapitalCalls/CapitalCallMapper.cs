using Meridian.Application.Common;
using Meridian.Domain.Entities;

namespace Meridian.Application.CapitalCalls;

public static class CapitalCallMapper
{
    public static CapitalCallDto ToDto(CapitalCall call) => new(
        call.Id,
        call.Ref,
        call.DealId,
        call.DealName,
        call.FundId,
        call.Tranche,
        call.Borrower,
        call.Amount,
        call.DueDate,
        call.CurrentStage,
        call.Status.ToDisplay(),
        call.Basis.ToDisplay(),
        call.PendingEscalations,
        call.Allocations
            .OrderByDescending(a => a.Amount).ThenBy(a => a.InvestorId)
            .Select(a => new CallAllocationDto(a.InvestorId, a.InvestorName, a.Commitment, a.Amount, a.WireStatus.ToString()))
            .ToList(),
        call.StageEvents
            .OrderBy(e => e.Stage)
            .Select(e => new StageEventDto(
                e.Stage, e.State.ToDisplay(), e.Actor,
                e.Date is { } d ? Display.ShortDate(d) : null, e.Note, e.Comment))
            .ToList(),
        call.Documents
            .OrderBy(d => d.Date).ThenBy(d => d.Name)
            .Select(d => new CallDocumentDto(d.Name, d.By, Display.ShortDate(d.Date)))
            .ToList(),
        call.AuditEntries
            .OrderByDescending(a => a.At)
            .Select(a => new CallAuditDto(a.Title, a.By, Display.ShortDateTime(a.At), a.Comment, a.Tone))
            .ToList());
}
