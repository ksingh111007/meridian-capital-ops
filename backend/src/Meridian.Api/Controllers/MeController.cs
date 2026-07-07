using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

public sealed record CurrentUserDto(
    string Id, string Name, string Initials, string Role, IReadOnlyDictionary<string, string> Capabilities);

[ApiController]
[Route("api/me")]
public class MeController(ICurrentUserProvider currentUser) : ControllerBase
{
    /// <summary>
    /// GET /api/me — drives approval affordances in the UI (docs/API.md § Session).
    /// Carries the caller's own capability matrix so screens never need the
    /// Admin-gated user directory to decide what to offer.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CurrentUserDto>> Get(CancellationToken ct)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        return new CurrentUserDto(user.Id, user.Name, user.Initials, user.RoleName,
            user.Capabilities.ToDictionary(c => c.Key.ToDisplay(), c => c.Value.ToDisplay()));
    }
}
