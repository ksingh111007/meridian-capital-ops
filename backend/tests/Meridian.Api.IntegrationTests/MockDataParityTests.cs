using System.Reflection;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>
/// The embedded seed JSONs (Infrastructure/Seeding/MockData) are copies of the
/// frontend mocks — copies because the Docker build context ends at backend/.
/// This pins them byte-identical to the originals so an edited mock can never
/// silently desync the SQLite seed from the frontend and the Azure SQL seed
/// (both generated from meridian-capital-ops/src/mocks).
/// </summary>
public class MockDataParityTests
{
    [Fact]
    public void EmbeddedSeedJsons_MatchTheFrontendMocks()
    {
        var mocksDir = FindFrontendMocks();
        if (mocksDir is null)
            return; // building outside the monorepo (e.g. Docker) — nothing to compare against

        var assembly = typeof(Meridian.Infrastructure.Seeding.MockDataSeed).Assembly;
        const string prefix = "Meridian.Infrastructure.Seeding.MockData.";
        var resources = assembly.GetManifestResourceNames().Where(n => n.StartsWith(prefix)).ToList();
        Assert.NotEmpty(resources);

        foreach (var resource in resources)
        {
            var fileName = resource[prefix.Length..];
            var original = Path.Combine(mocksDir, fileName);
            Assert.True(File.Exists(original), $"{fileName} is embedded but missing from src/mocks.");

            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            Assert.True(reader.ReadToEnd() == File.ReadAllText(original),
                $"{fileName} differs from meridian-capital-ops/src/mocks — re-copy it into Seeding/MockData "
                + "and regenerate database/scripts/seed (node database/tools/generate-seed.mjs).");
        }
    }

    private static string? FindFrontendMocks()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "meridian-capital-ops", "src", "mocks");
            if (Directory.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
