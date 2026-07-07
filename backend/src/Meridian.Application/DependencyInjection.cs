using Meridian.Application.Admin;
using Meridian.Application.Attention;
using Meridian.Application.Audit;
using Meridian.Application.Automation;
using Meridian.Application.CapitalCalls;
using Meridian.Application.Distributions;
using Meridian.Application.FundOps;
using Meridian.Application.Portal;
using Meridian.Application.Portfolio;
using Meridian.Application.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CapitalCallService>();
        services.AddScoped<WorkflowService>();
        services.AddScoped<DistributionService>();
        services.AddScoped<NeedsAttentionService>();
        services.AddScoped<AuditLogService>();
        services.AddScoped<OverdueSweepService>();
        services.AddScoped<ApprovalSlaService>();
        services.AddScoped<CustodianSyncService>();
        services.AddScoped<PortfolioService>();
        services.AddScoped<DrawdownService>();
        services.AddScoped<WireService>();
        services.AddScoped<CashPositionService>();
        services.AddScoped<ReconciliationService>();
        services.AddScoped<AdminDirectoryService>();
        services.AddScoped<FundAdminService>();
        services.AddScoped<PlatformConfigService>();
        services.AddScoped<PortalService>();
        return services;
    }
}
