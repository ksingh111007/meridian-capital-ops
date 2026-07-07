using Meridian.Application.Attention;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/needs-attention")]
public class NeedsAttentionController(NeedsAttentionService service) : ControllerBase
{
    /// <summary>Computed per caller from live state — pending approvals, overdue wires, calls due ≤ 7d.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttentionItemDto>>> Get(CancellationToken ct) =>
        Ok(await service.ComputeAsync(ct));
}
