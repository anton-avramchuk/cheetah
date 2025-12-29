namespace Cheetah.Core.EntityFramework.Migrations;

/// <summary>
/// Interface for database migration execution
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Apply pending migrations to the database
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
