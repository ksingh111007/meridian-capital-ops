using Meridian.Api.Auth;
using Meridian.Application.FundOps;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

public sealed record AssignReconRequest(string Assignee);

[ApiController]
public class FundOpsController(
    DrawdownService drawdowns, WireService wires, CashPositionService cash, ReconciliationService reconciliation)
    : ControllerBase
{
    [HttpGet("api/drawdowns")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<DrawdownsDto>> Drawdowns(CancellationToken ct) =>
        Ok(await drawdowns.GetAsync(ct));

    [HttpGet("api/wires")]
    [RequireCapability(ModuleName.Wires, Capability.View)]
    public async Task<ActionResult<WiresDto>> Wires(CancellationToken ct) =>
        Ok(await wires.GetAsync(ct));

    [HttpPost("api/wires/{id}/retry")]
    [RequireCapability(ModuleName.Wires, Capability.Edit)]
    public async Task<ActionResult<WireDto>> RetryWire(string id, CancellationToken ct) =>
        Ok(await wires.RetryAsync(id, ct));

    [HttpGet("api/cash/position")]
    [RequireCapability(ModuleName.Wires, Capability.View)]
    public async Task<ActionResult<CashPositionDto>> CashPosition(CancellationToken ct) =>
        Ok(await cash.GetAsync(ct));

    [HttpGet("api/reconciliation")]
    [RequireCapability(ModuleName.Recon, Capability.View)]
    public async Task<ActionResult<ReconciliationDto>> Reconciliation(CancellationToken ct) =>
        Ok(await reconciliation.GetAsync(ct));

    [HttpPost("api/reconciliation/{id}/assign")]
    [RequireCapability(ModuleName.Recon, Capability.Edit)]
    public async Task<ActionResult<ReconItemDto>> AssignRecon(string id, [FromBody] AssignReconRequest request, CancellationToken ct) =>
        Ok(await reconciliation.AssignAsync(id, request.Assignee, ct));
}
