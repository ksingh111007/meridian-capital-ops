using Meridian.Api.Auth;
using Meridian.Application.Distributions;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/distributions")]
public class DistributionsController(DistributionService service) : ControllerBase
{
    [HttpGet]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<IReadOnlyList<DistributionDto>>> List(CancellationToken ct) =>
        Ok(await service.ListAsync(ct));

    [HttpGet("{id}")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<DistributionDto>> Get(string id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));
}
