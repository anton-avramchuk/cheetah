namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Resolved, immutable table/column metadata for an entity type.
/// Produced by <see cref="IDapperEntityMap"/> (explicit) or by convention,
/// and cached by <see cref="IEntityMapRegistry"/>.
/// </summary>
public sealed class EntityMapping
{
    /// <summary>The mapped CLR entity type.</summary>
    public required Type EntityType { get; init; }

    /// <summary>The table name (unquoted).</summary>
    public required string TableName { get; init; }

    /// <summary>Optional schema name (unquoted); <c>null</c> for the provider default.</summary>
    public string? Schema { get; init; }

    /// <summary>The primary-key column.</summary>
    public required ColumnMapping Key { get; init; }

    /// <summary>All mapped columns, including the key.</summary>
    public required IReadOnlyList<ColumnMapping> Columns { get; init; }

    /// <summary>Columns written by <c>INSERT</c> (excludes database-generated columns).</summary>
    public IReadOnlyList<ColumnMapping> InsertableColumns
        => Columns.Where(c => !c.IsDatabaseGenerated).ToList();

    /// <summary>Columns written by <c>UPDATE</c> (excludes key and generated columns).</summary>
    public IReadOnlyList<ColumnMapping> UpdatableColumns
        => Columns.Where(c => !c.IsKey && !c.IsDatabaseGenerated).ToList();
}
