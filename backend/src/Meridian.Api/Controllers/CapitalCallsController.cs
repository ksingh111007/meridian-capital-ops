using Meridian.Api.Auth;
using Meridian.Application.CapitalCalls;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/capital-calls")]
public class CapitalCallsController(CapitalCallService service) : ControllerBase
{
    [HttpGet]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<IReadOnlyList<CapitalCallDto>>> List(CancellationToken ct) =>
        Ok(await service.ListAsync(ct));

    [HttpGet("{id}")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<CapitalCallDto>> Get(string id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));

    /// <summary>Create (wizard 2c). Re-validates Σ allocations = amount server-side; lands at stage 1 with notices queued.</summary>
    [HttpPost]
    [RequireCapability(ModuleName.Blotter, Capability.Edit)]
    public async Task<ActionResult<CapitalCallDto>> Create(CreateCapitalCallRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Approve the current stage (comment required). Stage-approver match is enforced in the workflow.</summary>
    [HttpPost("{id}/approve")]
    [RequireCapability(ModuleName.Approvals, Capability.Approve)]
    public async Task<ActionResult<ApprovalResultDto>> Approve(string id, CallActionRequest request, CancellationToken ct) =>
        Ok(await service.ApproveAsync(id, request.Comment, ct));

    /// <summary>Reject the current stage (comment required); returns the call to the prior stage as Returned.</summary>
    [HttpPost("{id}/reject")]
    [RequireCapability(ModuleName.Approvals, Capability.Approve)]
    public async Task<ActionResult<ApprovalResultDto>> Reject(string id, CallActionRequest request, CancellationToken ct) =>
        Ok(await service.RejectAsync(id, request.Comment, ct));
}
