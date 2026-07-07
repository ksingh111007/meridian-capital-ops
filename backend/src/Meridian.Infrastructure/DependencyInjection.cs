using Meridian.Application.Abstractions;
using Meridian.Infrastructure.ExternalServices;
using Meridian.Infrastructure.Jobs;
using Meridian.Infrastructure.Persistence;
using Meridian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Meridian.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "DataSource=meridian;Mode=Memory;Cache=Shared";

        if (UseSqlServer(configuration))
        {
            // Azure SQL. Schema and seed data are owned by the database/ dacpac
            // project — the API never creates or migrates objects here.
            services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
            services.AddDbContext<AppDbContext>((sp, options) => options
                .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
                .AddInterceptors(sp.GetRequiredService<AuditColumnsInterceptor>()));
        }
        else
        {
            // Dev/test store: shared-cache in-memory SQLite, created + seeded on boot.
            services.AddSingleton(_ => new SqliteConnectionFactory(connectionString));
            services.AddSingleton<IDbConnectionFactory>(sp => sp.GetRequiredService<SqliteConnectionFactory>());
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                // Touch the factory first: its keep-alive connection must exist before
                // EF opens against the shared in-memory database.
                _ = sp.GetRequiredService<SqliteConnectionFactory>();
                options.UseSqlite(connectionString)
                    .AddInterceptors(sp.GetRequiredService<AuditColumnsInterceptor>());
            });
        }

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<AuditColumnsInterceptor>();
        services.AddScoped<DatabaseInitializer>();
        services.TryAddScoped<IAuditActorProvider, SystemAuditActorProvider>();

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

    /// <summary>Database:Provider selects the store — "SqlServer" for Azure SQL, default SQLite.</summary>
    public static bool UseSqlServer(IConfiguration configuration) =>
        string.Equals(configuration["Database:Provider"], "SqlServer", StringComparison.OrdinalIgnoreCase);

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
