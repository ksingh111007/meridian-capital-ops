namespace Meridian.Application.Abstractions;

/// <summary>
/// Injected time — never DateTime.Now in business code. The dev/test host pins
/// the business date to 2026-07-05 to match the frontend mock story.
/// </summary>
public interface IClock
{
    DateOnly Today { get; }
    DateTime UtcNow { get; }
}
