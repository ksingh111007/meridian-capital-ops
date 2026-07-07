using Meridian.Application.Abstractions;
using Meridian.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Meridian.Api.Auth;

// Server-side enforcement of the role → capability matrix (BUSINESS_RULES.md § RBAC).
// Usage: [RequireCapability(ModuleName.Approvals, Capability.Approve)].
// Capabilities are ordered, so Edit satisfies View, Full satisfies everything.

public sealed class RequireCapabilityAttribute : AuthorizeAttribute
{
    public RequireCapabilityAttribute(ModuleName module, Capability capability) =>
        Policy = CapabilityPolicies.Name(module, capability);
}

public sealed class CapabilityRequirement(ModuleName module, Capability capability) : IAuthorizationRequirement
{
    public ModuleName Module { get; } = module;
    public Capability Capability { get; } = capability;
}

public static class CapabilityPolicies
{
    private const string Prefix = "capability:";

    public static string Name(ModuleName module, Capability capability) => $"{Prefix}{module}:{capability}";

    public static bool TryParse(string policyName, out CapabilityRequirement requirement)
    {
        requirement = null!;
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal))
            return false;
        var parts = policyName[Prefix.Length..].Split(':');
        if (parts.Length != 2
            || !Enum.TryParse<ModuleName>(parts[0], out var module)
            || !Enum.TryParse<Capability>(parts[1], out var capability))
        {
            return false;
        }

        requirement = new CapabilityRequirement(module, capability);
        return true;
    }
}

/// <summary>Materializes "capability:Module:Level" policy names on demand.</summary>
public sealed class CapabilityPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!CapabilityPolicies.TryParse(policyName, out var requirement))
            return _fallback.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(requirement)
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

public sealed class CapabilityAuthorizationHandler : AuthorizationHandler<CapabilityRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, CapabilityRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true || context.Resource is not HttpContext httpContext)
            return;

        var currentUser = await httpContext.RequestServices
            .GetRequiredService<ICurrentUserProvider>()
            .GetRequiredAsync(httpContext.RequestAborted);

        if (currentUser.HasAtLeast(requirement.Module, requirement.Capability))
            context.Succeed(requirement);
    }
}
