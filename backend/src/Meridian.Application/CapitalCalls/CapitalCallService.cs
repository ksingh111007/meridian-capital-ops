using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.CapitalCalls;

public class CapitalCallService(
    IAppDbContext db,
    IClock clock,
    ICurrentUserProvider currentUser,
    IAuditTrail audit,
    INotificationService notifications)
{
    public async Task<IReadOnlyList<CapitalCallDto>> ListAsync(CancellationToken ct = default)
    {
        var calls = await db.CapitalCalls.AsNoTracking().ToListAsync(ct);
        return calls.OrderByDescending(c => c.Id).Select(CapitalCallMapper.ToDto).ToList();
    }

    public async Task<CapitalCallDto> GetAsync(string id, CancellationToken ct = default)
    {
        var call = await db.CapitalCalls.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw DomainException.NotFound($"Capital call '{id}' was not found.");
        return CapitalCallMapper.ToDto(call);
    }

    public async Task<CapitalCallDto> CreateAsync(CreateCapitalCallRequest request, CancellationToken ct = default)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        var basis = ParseBasis(request.Basis);

        if (request.Amount <= 0 || !AllocationCalculator.HasAtMostTwoDecimals(request.Amount))
            throw DomainException.Validation("Call amount must be positive, in USD millions with at most 2 decimal places.");
        if (request.DueDate < clock.Today)
            throw DomainException.Validation("Due date cannot be in the past.");
        if (request.Allocations is null || request.Allocations.Count == 0)
            throw DomainException.Validation("At least one allocation is required.");

        var deal = await db.Deals.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.DealId, ct)
            ?? throw DomainException.Validation($"Unknown deal '{request.DealId}'.");

        var allocations = await BuildAllocationsAsync(request, deal, ct);

        var stages = await db.WorkflowStages.AsNoTracking().OrderBy(s => s.Order).ToListAsync(ct);
        var rules = await db.EscalationRules.AsNoTracking().ToListAsync(ct);
        var (id, reference) = await NextIdAsync(ct);
        var now = clock.UtcNow;

        var call = new CapitalCall
        {
            Id = id,
            Ref = reference,
            DealId = deal.Id,
            DealName = deal.Name,
            FundId = deal.FundId,
            Tranche = deal.Tranche,
            Borrower = deal.Borrower,
            Amount = request.Amount,
            DueDate = request.DueDate,
            Basis = basis,
            CurrentStage = 1,
            Status = CallStatus.InReview,
            Allocations = allocations,
            StageEvents =
            [
                new StageEvent { Stage = 1, State = StageState.Current, Actor = user.Name, Date = clock.Today, Note = "In review" },
            ],
            Documents =
            [
                new CallDocument { Name = "Capital Call Notice.pdf", By = user.Name, Date = clock.Today },
            ],
            AuditEntries =
            [
                new CallAuditEntry { Title = "Call created", By = user.Name, At = now, Tone = "neutral" },
                new CallAuditEntry { Title = "Notices queued", By = "System", At = now.AddSeconds(1), Tone = "blue" },
            ],
        };

        var escalations = EscalationEvaluator.Apply(call, rules, stages);
        foreach (var rule in escalations)
        {
            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = $"Escalation triggered — {rule.Effect}",
                By = "System",
                At = now.AddSeconds(2),
                Tone = "amber",
            });
        }

        db.CapitalCalls.Add(call);
        await db.SaveChangesAsync(ct);

        await audit.AppendAsync(user.Name, "Created", "green",
            $"Call {call.Ref} · {call.DealName}",
            $"{Display.Money(call.Amount)} due {call.DueDate:yyyy-MM-dd} · basis {basis.ToDisplay()}", ct);
        foreach (var rule in escalations)
            await audit.AppendAsync("System", "Escalation", "amber", $"Call {call.Ref}", rule.Effect, ct);

        var firstStage = stages.First(s => s.Order == 1);
        await notifications.NotifyRoleAsync(firstStage.ApproverRole,
            $"Call {call.Ref} awaits {firstStage.Name} approval",
            $"{call.DealName} · {Display.Money(call.Amount)} · due {call.DueDate:yyyy-MM-dd}", ct);

        return CapitalCallMapper.ToDto(call);
    }

    public Task<ApprovalResultDto> ApproveAsync(string id, string? comment, CancellationToken ct = default) =>
        ActAsync(id, comment, approve: true, ct);

    public Task<ApprovalResultDto> RejectAsync(string id, string? comment, CancellationToken ct = default) =>
        ActAsync(id, comment, approve: false, ct);

    private async Task<ApprovalResultDto> ActAsync(string id, string? comment, bool approve, CancellationToken ct)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        var call = await db.CapitalCalls.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw DomainException.NotFound($"Capital call '{id}' was not found.");
        var stages = await db.WorkflowStages.AsNoTracking().OrderBy(s => s.Order).ToListAsync(ct);
        var actor = new Actor(user.Name, user.RoleName, user.HasAtLeast(ModuleName.Approvals, Capability.Full));

        if (approve)
        {
            var outcome = ApprovalWorkflow.Approve(call, stages, actor, comment, clock.Today, clock.UtcNow);
            await db.SaveChangesAsync(ct);

            var action = outcome.EscalationSignoffOnly ? "Escalation sign-off" : "Approved";
            await audit.AppendAsync(user.Name, action, "green",
                $"Call {call.Ref} · {outcome.ActedStageName}", Quote(comment), ct);

            if (outcome.Completed)
            {
                await notifications.NotifyRoleAsync("Ops Manager", $"Call {call.Ref} completed",
                    "All approval stages cleared; custodians notified.", ct);
            }
            else if (!outcome.EscalationSignoffOnly && outcome.NextApproverRole is { Length: > 0 } nextRole)
            {
                await notifications.NotifyRoleAsync(nextRole,
                    $"Call {call.Ref} awaits {outcome.NextStageName} approval",
                    $"{call.DealName} · {Display.Money(call.Amount)} · due {call.DueDate:yyyy-MM-dd}", ct);
            }

            return new ApprovalResultDto(CapitalCallMapper.ToDto(call), outcome.Completed,
                outcome.EscalationSignoffOnly, outcome.NextApproverRole);
        }

        var priorStageName = ApprovalWorkflow.Reject(call, stages, actor, comment, clock.Today, clock.UtcNow);
        await db.SaveChangesAsync(ct);
        await audit.AppendAsync(user.Name, "Returned", "amber",
            $"Call {call.Ref} · returned to {priorStageName}", Quote(comment), ct);
        var priorRole = stages.First(s => s.Order == call.CurrentStage).ApproverRole;
        await notifications.NotifyRoleAsync(priorRole, $"Call {call.Ref} returned to {priorStageName}",
            Quote(comment), ct);

        return new ApprovalResultDto(CapitalCallMapper.ToDto(call), false, false, priorRole);
    }

    private async Task<List<CallAllocation>> BuildAllocationsAsync(
        CreateCapitalCallRequest request, Deal deal, CancellationToken ct)
    {
        var investorIds = request.Allocations!.Select(a => a.InvestorId).ToList();
        if (investorIds.Distinct().Count() != investorIds.Count)
            throw DomainException.Validation("Duplicate investor in allocations.");

        var investors = await db.Investors.AsNoTracking()
            .Where(i => investorIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, ct);

        var allocations = new List<CallAllocation>();
        foreach (var line in request.Allocations!)
        {
            if (!investors.TryGetValue(line.InvestorId, out var investor))
                throw DomainException.Validation($"Unknown investor '{line.InvestorId}'.");
            var commitment = investor.Commitments.FirstOrDefault(c => c.FundId == deal.FundId)
                ?? throw DomainException.Validation($"{investor.Name} has no commitment in {deal.FundId}.");
            if (line.Amount <= 0 || !AllocationCalculator.HasAtMostTwoDecimals(line.Amount))
                throw DomainException.Validation($"Allocation for {investor.Name} must be positive with at most 2 decimal places.");

            allocations.Add(new CallAllocation
            {
                InvestorId = investor.Id,
                InvestorName = investor.Name,
                Commitment = commitment.Amount,
                Amount = line.Amount,
                WireStatus = WireStatus.Pending,
            });
        }

        // The reconciliation gate — enforced in the wizard UI and re-validated here.
        var total = allocations.Sum(a => a.Amount);
        if (total != request.Amount)
            throw DomainException.Validation(
                $"Allocations must reconcile to the call amount: allocated {Display.Money(total)}, call {Display.Money(request.Amount)}.");

        return allocations;
    }

    private async Task<(string Id, string Ref)> NextIdAsync(CancellationToken ct)
    {
        var ids = await db.CapitalCalls.AsNoTracking().Select(c => c.Id).ToListAsync(ct);
        var next = ids
            .Select(i => int.TryParse(i.Replace("call-", "", StringComparison.Ordinal), out var n) ? n : 0)
            .DefaultIfEmpty(2000)
            .Max() + 1;
        return ($"call-{next}", $"#C-{next}");
    }

    private static AllocationBasis ParseBasis(string? basis) => basis?.ToLowerInvariant() switch
    {
        "unfunded" => AllocationBasis.Unfunded,
        "commitment" => AllocationBasis.Commitment,
        _ => throw DomainException.Validation("Basis must be \"unfunded\" or \"commitment\"."),
    };

    private static string Quote(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? "" : $"“{comment}”";
}
