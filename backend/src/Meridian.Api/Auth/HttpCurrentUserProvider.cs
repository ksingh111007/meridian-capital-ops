using System.Security.Claims;
using Meridian.Application.Abstractions;
using Meridian.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Api.Auth;

/// <summary>Resolves the authenticated principal plus their role's capability matrix (cached per request).</summary>
public sealed class HttpCurrentUserProvider(IHttpContextAccessor httpContextAccessor, IAppDbContext db)
    : ICurrentUserProvider
{
    private CurrentUser? _cached;

    public async Task<CurrentUser> GetRequiredAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
            return _cached;

        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw DomainException.Forbidden("No authenticated user.");

        var roleName = principal.FindFirstValue(ClaimTypes.Role) ?? "";
        var role = await db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            ?? throw DomainException.Forbidden($"Role '{roleName}' has no capability matrix configured.");

        _cached = new CurrentUser(
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            principal.FindFirstValue(ClaimTypes.Name) ?? "",
            principal.FindFirstValue(HeaderAuthentication.InitialsClaim) ?? "",
            roleName,
            role.Capabilities);
        return _cached;
    }
}
