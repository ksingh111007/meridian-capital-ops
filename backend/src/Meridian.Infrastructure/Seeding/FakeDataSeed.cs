using Bogus;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Seeding;

/// <summary>
/// Volume data on top of the fixed story records, generated with Bogus under a
/// fixed seed so every run (and every test host) sees the same rows. Only
/// reference-ish entities are faked — capital calls and distributions stay
/// story-only so workflow state remains predictable.
/// </summary>
public static class FakeDataSeed
{
    private const int RandomSeed = 20260705;

    private static readonly string[] Sectors =
        ["Healthcare", "Technology", "Industrials", "Consumer", "Business Services", "Transportation"];

    private static readonly string[] Tranches = ["Term A", "Term B", "Unitranche", "Revolver"];

    private static readonly string[] FundIds = ["fund-i", "fund-ii", "fund-iii"];

    private static readonly string[] InvestorTypes =
        ["Public pension", "Endowment", "Insurance", "Family office", "Sovereign wealth", "Asset manager"];

    public static void Apply(AppDbContext db, int investors, int deals)
    {
        var faker = new Faker { Random = new Randomizer(RandomSeed) };

        for (var i = 1; i <= deals; i++)
        {
            var invested = Round2(faker.Random.Decimal(10m, 90m));
            db.Deals.Add(new Deal
            {
                Id = $"deal-fake-{i:d2}",
                Name = $"Project {faker.Address.City().Split(' ')[0]}",
                Borrower = faker.Company.CompanyName(),
                Sector = faker.PickRandom(Sectors),
                Country = faker.PickRandom("US", "UK", "DE", "NL", "CA"),
                FundId = faker.PickRandom(FundIds),
                Tranche = faker.PickRandom(Tranches),
                Invested = invested,
                Outstanding = Round2(invested * faker.Random.Decimal(0.6m, 1m)),
                Spread = $"S+{faker.Random.Decimal(2m, 5.5m):0.00}%",
                NetIrrPct = Round2(faker.Random.Decimal(6m, 16m)),
                IrrTrend = faker.PickRandom("up", "down", "flat"),
                Moic = Round2(faker.Random.Decimal(1m, 1.5m)),
                Status = faker.Random.WeightedRandom(["Performing", "Watch", "Non-accrual"], [0.8f, 0.15f, 0.05f]),
            });
        }

        for (var i = 1; i <= investors; i++)
        {
            var commitment = Round2(faker.Random.Decimal(5m, 80m));
            db.Investors.Add(new Investor
            {
                Id = $"inv-fake-{i:d2}",
                Name = faker.Company.CompanyName(),
                Type = faker.PickRandom(InvestorTypes),
                WireInstructionsOnFile = faker.Random.Float() > 0.1f,
                Commitments =
                [
                    new InvestorCommitment
                    {
                        FundId = faker.PickRandom(FundIds),
                        Amount = commitment,
                        Called = Round2(commitment * faker.Random.Decimal(0.3m, 0.95m)),
                    },
                ],
            });
        }
    }

    private static decimal Round2(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
