using System.Reflection;

namespace Cheetah.Core.Dapper.UnitOfWork;

/// <summary>
/// A buffered write (INSERT/UPDATE/DELETE) queued in the unit of work and flushed
/// atomically on <see cref="IDapperUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public sealed class PendingOperation
{
    /// <summary>Logical connection string name (<c>null</c> = default).</summary>
    public string? ConnectionName { get; init; }

    /// <summary>The SQL statement to execute.</summary>
    public required string Sql { get; init; }

    /// <summary>The parameter source (typically the entity instance).</summary>
    public required object Parameters { get; init; }

    /// <summary>
    /// When set, the statement returns a generated key value that must be written
    /// back to this property of <see cref="Parameters"/>.
    /// </summary>
    public PropertyInfo? GeneratedKeyProperty { get; init; }
}
