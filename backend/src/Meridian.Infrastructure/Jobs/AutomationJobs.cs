using Meridian.Application.Automation;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Meridian.Infrastructure.Jobs;

// Quartz job shells — thin wrappers so the schedulable unit stays separate from
// the business logic (which lives in Application and is tested directly).
// Jobs are durable: cron triggers come from configuration (Jobs:* keys) and the
// ops endpoint can fire any of them on demand.

[DisallowConcurrentExecution]
public sealed class OverdueAllocationSweepJob(OverdueSweepService service, ILogger<OverdueAllocationSweepJob> logger) : IJob
{
    public static readonly JobKey Key = new("overdue-allocation-sweep", "ops");

    public async Task Execute(IJobExecutionContext context)
    {
        var flipped = await service.SweepAsync(context.CancellationToken);
        logger.LogInformation("Overdue sweep: {Count} allocation(s) marked overdue.", flipped);
    }
}

[DisallowConcurrentExecution]
public sealed class ApprovalSlaMonitorJob(ApprovalSlaService service, ILogger<ApprovalSlaMonitorJob> logger) : IJob
{
    public static readonly JobKey Key = new("approval-sla-monitor", "ops");

    public async Task Execute(IJobExecutionContext context)
    {
        var breaches = await service.CheckAsync(context.CancellationToken);
        logger.LogInformation("Approval SLA monitor: {Count} stage(s) past SLA.", breaches);
    }
}

[DisallowConcurrentExecution]
public sealed class CustodianFeedSyncJob(CustodianSyncService service, ILogger<CustodianFeedSyncJob> logger) : IJob
{
    public static readonly JobKey Key = new("custodian-feed-sync", "ops");

    public async Task Execute(IJobExecutionContext context)
    {
        var records = await service.SyncAsync(context.CancellationToken);
        logger.LogInformation("Custodian feed sync: {Count} records ingested.", records);
    }
}
