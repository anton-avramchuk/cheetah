namespace Cheetah.Core.EntityFramework.Seeding;

/// <summary>
/// Interface for database seeding with default data
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seed order (lower values execute first)
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Seed the database with default data
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
