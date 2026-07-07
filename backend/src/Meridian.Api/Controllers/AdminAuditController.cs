using Meridian.Api.Auth;
using Meridian.Application.Audit;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/admin/audit")]
public class AdminAuditController(AuditLogService service) : ControllerBase
{
    /// <summary>Append-only, hash-chained audit log; kpis.chainValid re-verifies the chain.</summary>
    [HttpGet]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<AuditLogDto>> Get(CancellationToken ct) =>
        Ok(await service.GetAsync(ct));
}
