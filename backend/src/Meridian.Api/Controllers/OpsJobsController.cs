using Meridian.Api.Auth;
using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;

namespace Meridian.Api.Controllers;

public sealed record JobInfoDto(string Name, string Group, bool HasCronTrigger, DateTimeOffset? NextFireTimeUtc);

/// <summary>
/// Automation endpoints over the Quartz scheduler: inspect registered background
/// jobs and fire one on demand (e.g. run the overdue sweep right after a due date
/// passes instead of waiting for the cron tick).
/// </summary>
[ApiController]
[Route("api/ops/jobs")]
public class OpsJobsController(ISchedulerFactory schedulerFactory) : ControllerBase
{
    [HttpGet]
    [RequireCapability(ModuleName.Admin, Capability.View)]
    public async Task<ActionResult<IReadOnlyList<JobInfoDto>>> List(CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var jobs = new List<JobInfoDto>();
        foreach (var key in await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), ct))
        {
            var triggers = await scheduler.GetTriggersOfJob(key, ct);
            jobs.Add(new JobInfoDto(
                key.Name,
                key.Group,
                triggers.Count > 0,
                triggers.Select(t => t.GetNextFireTimeUtc()).Where(t => t is not null).Min()));
        }

        return Ok(jobs.OrderBy(j => j.Name).ToList());
    }

    [HttpPost("{jobName}/run")]
    [RequireCapability(ModuleName.Admin, Capability.Edit)]
    public async Task<IActionResult> Run(string jobName, CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var key = new JobKey(jobName, "ops");
        if (!await scheduler.CheckExists(key, ct))
            throw DomainException.NotFound($"Unknown job '{jobName}'.");

        await scheduler.TriggerJob(key, ct);
        return Accepted(new { triggered = jobName });
    }
}
