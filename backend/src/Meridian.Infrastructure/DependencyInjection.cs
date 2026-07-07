using Meridian.Application.Abstractions;
using Meridian.Infrastructure.ExternalServices;
using Meridian.Infrastructure.Jobs;
using Meridian.Infrastructure.Persistence;
using Meridian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Meridian.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "DataSource=meridian;Mode=Memory;Cache=Shared";

        services.AddSingleton(_ => new SqliteConnectionFactory(connectionString));
        services.AddSingleton<IDbConnectionFactory>(sp => sp.GetRequiredService<SqliteConnectionFactory>());
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            // Touch the factory first: its keep-alive connection must exist before
            // EF opens against the shared in-memory database.
            _ = sp.GetRequiredService<SqliteConnectionFactory>();
            options.UseSqlite(connectionString);
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DatabaseInitializer>();

        services.AddSingleton<IClock, ConfigurableClock>();
        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddScoped<INotificationService, LoggingNotificationService>();
        services.AddSingleton<IWireGateway, NoopWireGateway>();
        services.AddSingleton<ICustodianFeed, StaticCustodianFeed>();
        services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();

        services.AddQuartz(quartz =>
        {
            Schedule<OverdueAllocationSweepJob>(quartz, OverdueAllocationSweepJob.Key, configuration["Jobs:OverdueSweepCron"]);
            Schedule<ApprovalSlaMonitorJob>(quartz, ApprovalSlaMonitorJob.Key, configuration["Jobs:SlaMonitorCron"]);
            Schedule<CustodianFeedSyncJob>(quartz, CustodianFeedSyncJob.Key, configuration["Jobs:CustodianFeedSyncCron"]);
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }

    private static void Schedule<TJob>(IServiceCollectionQuartzConfigurator quartz, JobKey key, string? cron)
        where TJob : IJob
    {
        quartz.AddJob<TJob>(job => job.WithIdentity(key).StoreDurably());
        if (!string.IsNullOrWhiteSpace(cron))
        {
            quartz.AddTrigger(trigger => trigger
                .ForJob(key)
                .WithIdentity($"{key.Name}-cron", key.Group)
                .WithCronSchedule(cron));
        }
    }
}
