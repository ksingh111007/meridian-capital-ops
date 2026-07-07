using Meridian.Api.Auth;
using Meridian.Application.Attention;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/needs-attention")]
public class NeedsAttentionController(NeedsAttentionService service) : ControllerBase
{
    /// <summary>
    /// Computed per caller from live state — pending approvals, overdue wires, calls
    /// due ≤ 7d. Gated like the blotter: the items expose the same call data.
    /// </summary>
    [HttpGet]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<IReadOnlyList<AttentionItemDto>>> Get(CancellationToken ct) =>
        Ok(await service.ComputeAsync(ct));
}
