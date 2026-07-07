namespace Meridian.Application.CapitalCalls;

// Response shapes mirror CapitalCall & friends in the frontend's src/lib/types.ts
// (camelCase over the wire). Extra fields (basis, pendingEscalations) are additive.

public sealed record CallAllocationDto(
    string InvestorId, string Investor, decimal Commitment, decimal Amount, string WireStatus);

public sealed record StageEventDto(
    int Stage, string State, string? Actor, string? Date, string? Note, string? Comment);

public sealed record CallDocumentDto(string Name, string By, string Date);

public sealed record CallAuditDto(string Title, string By, string At, string? Comment, string Tone);

public sealed record CapitalCallDto(
    string Id,
    string Ref,
    string DealId,
    string Deal,
    string FundId,
    string Tranche,
    string Borrower,
    decimal Amount,
    DateOnly DueDate,
    int CurrentStage,
    string Status,
    string Basis,
    IReadOnlyList<string> PendingEscalations,
    IReadOnlyList<CallAllocationDto> Allocations,
    IReadOnlyList<StageEventDto> StageEvents,
    IReadOnlyList<CallDocumentDto> Documents,
    IReadOnlyList<CallAuditDto> Audit);

public sealed record CreateAllocationRequest(string InvestorId, decimal Amount);

public sealed record CreateCapitalCallRequest(
    string DealId,
    decimal Amount,
    DateOnly DueDate,
    string Basis,
    IReadOnlyList<CreateAllocationRequest>? Allocations);

public sealed record CallActionRequest(string? Comment);

public sealed record ApprovalResultDto(
    CapitalCallDto Call, bool Completed, bool EscalationSignoff, string? NextApproverRole);
