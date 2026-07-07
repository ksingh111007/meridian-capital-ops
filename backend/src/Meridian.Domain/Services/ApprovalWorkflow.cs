using Meridian.Domain.Entities;

namespace Meridian.Domain.Services;

public sealed record Actor(string Name, string Role, bool HasFullApprovals);

public sealed record ApprovalOutcome(
    bool EscalationSignoffOnly,
    bool Completed,
    string ActedStageName,
    string? NextStageName,
    string? NextApproverRole,
    IReadOnlyList<string> AutoAdvancedStages);

/// <summary>
/// The due-diligence approval pipeline (BUSINESS_RULES.md § Due-diligence approval pipeline).
/// Pure state transitions on a <see cref="CapitalCall"/>; persistence, global audit and
/// notifications are the application layer's job.
/// </summary>
public static class ApprovalWorkflow
{
    public static ApprovalOutcome Approve(
        CapitalCall call, IReadOnlyList<WorkflowStage> stages, Actor actor, string? comment, DateOnly today, DateTime now)
    {
        var stage = StageAt(stages, call.CurrentStage);
        EnsureActionable(call, stage, comment);

        // Escalation sign-off: an injected role (e.g. Compliance) approves without
        // owning the current stage and without advancing it.
        if (call.EscalationSignoffs.Any(s => s.Role == actor.Role) && actor.Role != stage.ApproverRole)
        {
            call.EscalationSignoffs.RemoveAll(s => s.Role == actor.Role);
            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = $"Escalation sign-off — {actor.Role}",
                By = actor.Name,
                At = now,
                Comment = comment,
                Tone = "green",
            });
            return new ApprovalOutcome(true, false, stage.Name, stage.Name, stage.ApproverRole, []);
        }

        if (actor.Role != stage.ApproverRole && !actor.HasFullApprovals)
            throw DomainException.Forbidden($"Stage {stage.Order} ({stage.Name}) requires the {stage.ApproverRole} role.");

        // A stage approver who is also an injected escalation role clears their own entry.
        call.EscalationSignoffs.RemoveAll(s => s.Role == actor.Role);
        // Each sign-off carries its own gate, so one rule can never weaken another's.
        var blocking = call.EscalationSignoffs
            .Where(s => stage.Order >= s.GateStage)
            .Select(s => s.Role)
            .Distinct()
            .ToList();
        if (blocking.Count > 0)
            throw DomainException.Conflict(
                $"Escalation sign-off outstanding before the call can pass {stage.Name}: {string.Join(", ", blocking)}.");

        MarkStage(call, stage.Order, StageState.Done, actor.Name, today, comment: comment);
        call.AuditEntries.Add(new CallAuditEntry
        {
            Title = $"{stage.Name} approved",
            By = actor.Name,
            At = now,
            Comment = comment,
            Tone = "green",
        });

        var autoAdvanced = new List<string>();
        var next = StageAt(stages, stage.Order + 1);
        while (next.AutoAdvance)
        {
            MarkStage(call, next.Order, StageState.Done, "System", today, note: "Auto");
            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = $"{next.Name} — completed", By = "System", At = now, Tone = "blue",
            });
            autoAdvanced.Add(next.Name);
            next = StageAt(stages, next.Order + 1);
        }

        call.CurrentStage = next.Order;
        if (next.Terminal)
        {
            // The terminal stage gets its own done event and audit entry — a completed
            // call's timeline and trail must show all nine stages (mock #C-2036 does).
            MarkStage(call, next.Order, StageState.Done, "System", today);
            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = "Call completed", By = "System", At = now, Tone = "green",
            });
            call.Status = CallStatus.Completed;
            return new ApprovalOutcome(false, true, stage.Name, next.Name, null, autoAdvanced);
        }

        call.Status = CallStatus.InReview;
        MarkStage(call, next.Order, StageState.Current, actor: null, date: today, note: "In review");
        return new ApprovalOutcome(false, false, stage.Name, next.Name, next.ApproverRole, autoAdvanced);
    }

    public static string Reject(
        CapitalCall call, IReadOnlyList<WorkflowStage> stages, Actor actor, string? comment, DateOnly today, DateTime now)
    {
        var stage = StageAt(stages, call.CurrentStage);
        EnsureActionable(call, stage, comment);

        if (actor.Role != stage.ApproverRole && !actor.HasFullApprovals)
            throw DomainException.Forbidden($"Stage {stage.Order} ({stage.Name}) requires the {stage.ApproverRole} role.");
        if (stage.Order <= 1)
            throw DomainException.Conflict("The call is at the first stage; withdraw it instead of rejecting.");

        var prior = StageAt(stages, stage.Order - 1);
        call.StageEvents.RemoveAll(e => e.Stage == stage.Order);
        MarkStage(call, prior.Order, StageState.Current, actor: null, date: today, note: $"Returned from {stage.Name}");
        call.CurrentStage = prior.Order;
        call.Status = CallStatus.Returned;
        call.AuditEntries.Add(new CallAuditEntry
        {
            Title = $"{stage.Name} returned the call to {prior.Name}",
            By = actor.Name,
            At = now,
            Comment = comment,
            Tone = "amber",
        });
        return prior.Name;
    }

    private static void EnsureActionable(CapitalCall call, WorkflowStage stage, string? comment)
    {
        if (call.Status == CallStatus.Completed || stage.Terminal)
            throw DomainException.Conflict($"Call {call.Ref} is already completed.");
        if (stage.ApproverRole is "System" or "")
            throw DomainException.Conflict($"Stage {stage.Order} ({stage.Name}) is automated and cannot be actioned manually.");
        if (string.IsNullOrWhiteSpace(comment))
            throw DomainException.Validation("A comment is required to approve or reject a capital call.");
    }

    private static WorkflowStage StageAt(IReadOnlyList<WorkflowStage> stages, int order) =>
        stages.FirstOrDefault(s => s.Order == order)
        ?? throw DomainException.Conflict($"Workflow has no stage {order}.");

    private static void MarkStage(
        CapitalCall call, int order, StageState state, string? actor, DateOnly date, string? note = null, string? comment = null)
    {
        var evt = call.StageEvents.FirstOrDefault(e => e.Stage == order);
        if (evt is null)
        {
            evt = new StageEvent { Stage = order };
            call.StageEvents.Add(evt);
        }

        evt.State = state;
        evt.Date = date;
        if (actor is not null) evt.Actor = actor;
        evt.Note = note;
        if (comment is not null) evt.Comment = comment;
    }
}
