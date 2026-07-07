using Meridian.Application.Abstractions;

namespace Meridian.Api.Auth;

/// <summary>Stamps row-level audit columns with the authenticated principal's name.</summary>
public sealed class HttpAuditActorProvider(IHttpContextAccessor httpContextAccessor) : IAuditActorProvider
{
    public string CurrentActor =>
        httpContextAccessor.HttpContext?.User.Identity is { IsAuthenticated: true, Name: { Length: > 0 } name }
            ? name
            : "system";
}
