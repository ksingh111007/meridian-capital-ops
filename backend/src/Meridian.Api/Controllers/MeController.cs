using Meridian.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

public sealed record CurrentUserDto(string Id, string Name, string Initials, string Role);

[ApiController]
[Route("api/me")]
public class MeController(ICurrentUserProvider currentUser) : ControllerBase
{
    /// <summary>GET /api/me — drives approval affordances in the UI (docs/API.md § Session).</summary>
    [HttpGet]
    public async Task<ActionResult<CurrentUserDto>> Get(CancellationToken ct)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        return new CurrentUserDto(user.Id, user.Name, user.Initials, user.RoleName);
    }
}
