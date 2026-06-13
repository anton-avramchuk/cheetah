namespace Cheetah.Core.Dapper.Sql;

/// <summary>
/// Precomputed SQL statement templates for a single entity type.
/// </summary>
public sealed class EntitySql
{
    /// <summary><c>SELECT col AS Prop, ... FROM table</c> (no WHERE).</summary>
    public required string SelectBase { get; init; }

    /// <summary><c>SELECT COUNT(1) FROM table</c> (no WHERE).</summary>
    public required string CountBase { get; init; }

    /// <summary>Full select of a single row by key.</summary>
    public required string SelectById { get; init; }

    /// <summary><c>INSERT</c> statement; returns the key when database-generated.</summary>
    public required string Insert { get; init; }

    /// <summary>Whether <see cref="Insert"/> returns the generated key value.</summary>
    public required bool InsertReturnsKey { get; init; }

    /// <summary><c>UPDATE</c> by key statement.</summary>
    public required string Update { get; init; }

    /// <summary><c>DELETE</c> by key statement.</summary>
    public required string DeleteById { get; init; }

    /// <summary>The quoted key column name.</summary>
    public required string KeyColumn { get; init; }

    /// <summary>The parameter name carrying the key value (without prefix).</summary>
    public required string KeyParameterName { get; init; }
}
