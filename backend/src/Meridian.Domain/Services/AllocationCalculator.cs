namespace Meridian.Domain.Services;

/// <summary>
/// Pro-rata capital-call allocation (BUSINESS_RULES.md § Capital-call allocation).
/// Amounts are USD millions with 2 decimal places; rounding uses the largest-remainder
/// method so the allocations always reconcile exactly to the call amount.
/// </summary>
public static class AllocationCalculator
{
    public static bool HasAtMostTwoDecimals(decimal value) => decimal.Round(value, 2) == value;

    public static IReadOnlyList<(string InvestorId, decimal Amount)> ProRata(
        decimal callAmount,
        IReadOnlyList<(string InvestorId, decimal Basis)> bases)
    {
        if (callAmount <= 0)
            throw DomainException.Validation("Call amount must be positive.");
        if (!HasAtMostTwoDecimals(callAmount))
            throw DomainException.Validation("Amounts are USD millions with at most 2 decimal places.");
        if (bases.Count == 0)
            throw DomainException.Validation("At least one investor is required.");
        if (bases.Any(b => b.Basis < 0))
            throw DomainException.Validation("Allocation bases cannot be negative.");

        var totalBasis = bases.Sum(b => b.Basis);
        if (totalBasis <= 0)
            throw DomainException.Validation("Total allocation basis must be positive.");

        // Floor each share to whole cents, then hand the remaining cents to the
        // largest fractional remainders (ties broken by input order) so the sum
        // is exact — never trust independent rounding with money.
        var flooredCents = new long[bases.Count];
        var remainders = new decimal[bases.Count];
        foreach (var (b, i) in bases.Select((b, i) => (b, i)))
        {
            var exactCents = callAmount * 100m * b.Basis / totalBasis;
            flooredCents[i] = (long)Math.Floor(exactCents);
            remainders[i] = exactCents - flooredCents[i];
        }

        var shortfall = (long)(callAmount * 100m) - flooredCents.Sum();
        foreach (var i in Enumerable.Range(0, bases.Count)
                     .OrderByDescending(i => remainders[i])
                     .ThenBy(i => i)
                     .Take((int)shortfall))
        {
            flooredCents[i]++;
        }

        return bases.Select((b, i) => (b.InvestorId, flooredCents[i] / 100m)).ToList();
    }
}
