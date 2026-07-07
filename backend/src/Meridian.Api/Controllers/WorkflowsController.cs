using Meridian.Api.Auth;
using Meridian.Application.Workflows;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/workflows")]
public class WorkflowsController(WorkflowService service) : ControllerBase
{
    [HttpGet("capital-calls")]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public async Task<ActionResult<WorkflowDto>> GetCapitalCallWorkflow(CancellationToken ct) =>
        Ok(await service.GetCapitalCallWorkflowAsync(ct));
}
