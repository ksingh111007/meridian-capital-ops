using Meridian.Application.Portal;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

/// <summary>
/// Investor-portal endpoints. No capability policy applies here — access is
/// gated by the portal-contact session (staff principals are rejected by the
/// portal session provider), and every read is scoped to the session's LP.
/// </summary>
[ApiController]
[Route("api/portal")]
public class PortalController(PortalService service) : ControllerBase
{
    [HttpGet("session")]
    public async Task<ActionResult<PortalSessionDto>> Session(CancellationToken ct) =>
        Ok(await service.GetSessionAsync(ct));

    [HttpGet("account")]
    public async Task<ActionResult<PortalAccountDto>> Account(CancellationToken ct) =>
        Ok(await service.GetAccountAsync(ct));

    [HttpGet("investments")]
    public async Task<ActionResult<PortalInvestmentsDto>> Investments(CancellationToken ct) =>
        Ok(await service.GetInvestmentsAsync(ct));

    [HttpGet("activity")]
    public async Task<ActionResult<PortalActivityDto>> Activity(CancellationToken ct) =>
        Ok(await service.GetActivityAsync(ct));

    [HttpGet("statements")]
    public async Task<ActionResult<PortalStatementsDto>> Statements(CancellationToken ct) =>
        Ok(await service.GetStatementsAsync(ct));

    [HttpGet("tax")]
    public async Task<ActionResult<PortalTaxDto>> Tax(CancellationToken ct) =>
        Ok(await service.GetTaxAsync(ct));

    [HttpGet("contact")]
    public async Task<ActionResult<PortalIrInfoDto>> Contact(CancellationToken ct) =>
        Ok(await service.GetIrInfoAsync(ct));

    [HttpPost("messages")]
    public async Task<ActionResult<IrRequestDto>> CreateMessage(
        [FromBody] CreateIrMessageRequest request, CancellationToken ct) =>
        Ok(await service.CreateMessageAsync(request, ct));
}
