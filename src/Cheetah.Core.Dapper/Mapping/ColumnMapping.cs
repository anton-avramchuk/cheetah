using System.Reflection;

namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Describes how a single entity property maps to a database column.
/// </summary>
public sealed class ColumnMapping
{
    /// <summary>The CLR property.</summary>
    public required PropertyInfo Property { get; init; }

    /// <summary>The CLR property name (used to materialize Dapper results).</summary>
    public string PropertyName => Property.Name;

    /// <summary>The database column name.</summary>
    public required string ColumnName { get; init; }

    /// <summary>True if this column is (part of) the primary key.</summary>
    public bool IsKey { get; init; }

    /// <summary>
    /// True if the value is produced by the database (identity / default / sequence)
    /// and therefore excluded from <c>INSERT</c> column lists.
    /// </summary>
    public bool IsDatabaseGenerated { get; init; }
}
