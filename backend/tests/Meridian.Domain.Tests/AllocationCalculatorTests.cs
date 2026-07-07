using Meridian.Domain;
using Meridian.Domain.Services;
using Xunit;

namespace Meridian.Domain.Tests;

public class AllocationCalculatorTests
{
    [Fact]
    public void ProRata_SplitsProportionallyAndReconcilesExactly()
    {
        var result = AllocationCalculator.ProRata(16.00m,
        [
            ("inv-a", 11.6m), // unfunded bases
            ("inv-b", 8.0m),
            ("inv-c", 17.1m),
        ]);

        Assert.Equal(16.00m, result.Sum(r => r.Amount));
        Assert.All(result, r => Assert.Equal(r.Amount, decimal.Round(r.Amount, 2)));
        // Largest basis gets the largest share.
        Assert.Equal("inv-c", result.OrderByDescending(r => r.Amount).First().InvestorId);
    }

    [Fact]
    public void ProRata_ThirdsStillSumExactly_LargestRemainderRounding()
    {
        var result = AllocationCalculator.ProRata(10.00m,
        [
            ("inv-a", 1m),
            ("inv-b", 1m),
            ("inv-c", 1m),
        ]);

        Assert.Equal(10.00m, result.Sum(r => r.Amount));
        Assert.Equal(3.34m, result.Max(r => r.Amount));
        Assert.Equal(3.33m, result.Min(r => r.Amount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ProRata_RejectsNonPositiveAmount(decimal amount)
    {
        var ex = Assert.Throws<DomainException>(() =>
            AllocationCalculator.ProRata(amount, [("inv-a", 1m)]));
        Assert.Equal(ErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public void ProRata_RejectsSubCentAmounts()
    {
        var ex = Assert.Throws<DomainException>(() =>
            AllocationCalculator.ProRata(10.005m, [("inv-a", 1m)]));
        Assert.Equal(ErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public void ProRata_RejectsZeroTotalBasis()
    {
        var ex = Assert.Throws<DomainException>(() =>
            AllocationCalculator.ProRata(10m, [("inv-a", 0m), ("inv-b", 0m)]));
        Assert.Equal(ErrorKind.Validation, ex.Kind);
    }
}
