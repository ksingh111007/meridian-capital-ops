using System.Globalization;
using Meridian.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Real UTC time by default. Setting <c>BusinessDate</c> (e.g. "2026-07-05" in
/// appsettings.Development.json) pins the business date so overdue/due-soon math
/// matches the seeded story — same trick as TODAY in the frontend's format.ts.
/// </summary>
public sealed class ConfigurableClock : IClock
{
    private readonly DateOnly? _fixedToday;

    public ConfigurableClock(IConfiguration configuration)
    {
        var setting = configuration["BusinessDate"];
        _fixedToday = string.IsNullOrWhiteSpace(setting)
            ? null
            : DateOnly.ParseExact(setting, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public DateOnly Today => _fixedToday ?? DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTime UtcNow => _fixedToday is { } day
        ? day.ToDateTime(TimeOnly.FromDateTime(DateTime.UtcNow), DateTimeKind.Utc)
        : DateTime.UtcNow;
}
