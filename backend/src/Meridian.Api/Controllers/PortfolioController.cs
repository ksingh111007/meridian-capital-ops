using Meridian.Api.Auth;
using Meridian.Application.Portfolio;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
public class PortfolioController(PortfolioService service) : ControllerBase
{
    [HttpGet("api/portfolio/summary")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<PortfolioSummaryDto>> Summary(CancellationToken ct) =>
        Ok(await service.GetSummaryAsync(ct));

    [HttpGet("api/deals")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<IReadOnlyList<DealDto>>> Deals(CancellationToken ct) =>
        Ok(await service.ListDealsAsync(ct));

    [HttpGet("api/deals/{id}")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<DealDetailDto>> Deal(string id, CancellationToken ct) =>
        Ok(await service.GetDealAsync(id, ct));
}
