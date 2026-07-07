using Meridian.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meridian.Infrastructure.Persistence;

public class DatabaseInitializer(AppDbContext db, IConfiguration configuration, ILogger<DatabaseInitializer> logger)
{
    public void Initialize()
    {
        if (db.Database.IsSqlServer())
        {
            // Azure SQL: schema (temporal tables, schemas, indexes) and seed data are
            // deployed by the database/ dacpac project — never create or seed from here.
            if (!db.Database.CanConnect())
                throw new InvalidOperationException(
                    "Cannot reach the configured SQL Server database. Deploy the database/ dacpac and check ConnectionStrings:Default.");
            logger.LogInformation("Using SQL Server database (schema managed by the database/ dacpac project).");
            return;
        }

        if (!db.Database.EnsureCreated())
            return; // Schema already exists — another host seeded this shared database.

        StorySeed.Apply(db);
        MockDataSeed.Apply(db);

        if (!bool.TryParse(configuration["Seed:IncludeFakeData"], out var includeFake) || includeFake)
        {
            FakeDataSeed.Apply(db,
                investors: ParseOr(configuration["Seed:FakeInvestors"], 8),
                deals: ParseOr(configuration["Seed:FakeDeals"], 10));
        }

        db.SaveChanges();
        logger.LogInformation("Database created and seeded ({Deals} deals, {Investors} investors).",
            db.Deals.Count(), db.Investors.Count());
    }

    private static int ParseOr(string? value, int fallback) =>
        int.TryParse(value, out var parsed) ? parsed : fallback;
}
