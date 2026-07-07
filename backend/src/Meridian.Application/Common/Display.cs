using System.Globalization;
using Meridian.Domain;

namespace Meridian.Application.Common;

/// <summary>
/// Formatting that matches the frontend contract in src/lib/types.ts:
/// statuses like "In Review", short dates like "Jul 02", money like "$16.00M".
/// </summary>
public static class Display
{
    public static string ToDisplay(this CallStatus status) =>
        status == CallStatus.InReview ? "In Review" : status.ToString();

    public static CallStatus ParseCallStatus(string value) =>
        value == "In Review" ? CallStatus.InReview : Enum.Parse<CallStatus>(value);

    public static string ToDisplay(this StageState state) => state.ToString().ToLowerInvariant();

    /// <summary>Module names as the frontend MODULES const spells them ("Ref Data").</summary>
    public static string ToDisplay(this ModuleName module) =>
        module == ModuleName.RefData ? "Ref Data" : module.ToString();

    public static string ToDisplay(this Capability capability) => capability.ToString().ToLowerInvariant();

    public static string ToDisplay(this AllocationBasis basis) => basis.ToString().ToLowerInvariant();

    public static string ShortDate(DateOnly date) => date.ToString("MMM dd", CultureInfo.InvariantCulture);

    public static string ShortDateTime(DateTime at) => at.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);

    public static string Money(decimal usdMillions) =>
        "$" + usdMillions.ToString("0.00", CultureInfo.InvariantCulture) + "M";
}
