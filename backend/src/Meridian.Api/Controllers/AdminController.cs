using Meridian.Api.Auth;
using Meridian.Application.Admin;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(
    AdminDirectoryService directory, FundAdminService fundAdmin, PlatformConfigService platformConfig)
    : ControllerBase
{
    [HttpGet("users")]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<UsersAndRolesDto>> Users(CancellationToken ct) =>
        Ok(await directory.GetUsersAndRolesAsync(ct));

    [HttpGet("funds")]
    [RequireCapability(ModuleName.RefData, Capability.View)]
    public async Task<ActionResult<FundsDto>> Funds(CancellationToken ct) =>
        Ok(await fundAdmin.GetFundsAsync(ct));

    [HttpGet("investors")]
    [RequireCapability(ModuleName.RefData, Capability.View)]
    public async Task<ActionResult<InvestorsDto>> Investors(CancellationToken ct) =>
        Ok(await fundAdmin.GetInvestorsAsync(ct));

    [HttpGet("reference")]
    [RequireCapability(ModuleName.RefData, Capability.View)]
    public async Task<ActionResult<ReferenceDataDto>> Reference(CancellationToken ct) =>
        Ok(await fundAdmin.GetReferenceDataAsync(ct));

    [HttpGet("integrations")]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<IntegrationsDto>> Integrations(CancellationToken ct) =>
        Ok(await platformConfig.GetIntegrationsAsync(ct));

    [HttpGet("notification-rules")]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<NotificationRulesDto>> NotificationRules(CancellationToken ct) =>
        Ok(await platformConfig.GetNotificationRulesAsync(ct));

    [HttpGet("investor-access")]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<InvestorAccessDto>> InvestorAccess(CancellationToken ct) =>
        Ok(await platformConfig.GetInvestorAccessAsync(ct));
}
