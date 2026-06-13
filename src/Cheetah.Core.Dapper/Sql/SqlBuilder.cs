using System.Collections.Concurrent;
using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.Sql;

/// <summary>
/// Default <see cref="ISqlBuilder"/>. Generates CRUD statements once per entity type and caches them.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISqlBuilder))]
public class SqlBuilder : ISqlBuilder
{
    private readonly ISqlDialect _dialect;
    private readonly ConcurrentDictionary<Type, EntitySql> _cache = new();

    /// <summary>Initializes a new instance of the <see cref="SqlBuilder"/> class.</summary>
    public SqlBuilder(ISqlDialect dialect) => _dialect = dialect;

    /// <inheritdoc />
    public EntitySql GetSql(EntityMapping mapping) => _cache.GetOrAdd(mapping.EntityType, _ => Build(mapping));

    private EntitySql Build(EntityMapping mapping)
    {
        var table = QuoteTable(mapping);
        var prefix = _dialect.ParameterPrefix;
        var keyColumn = _dialect.QuoteIdentifier(mapping.Key.ColumnName);
        var keyParam = mapping.Key.PropertyName;

        var selectList = string.Join(", ", mapping.Columns.Select(SelectColumn));
        var selectBase = $"SELECT {selectList} FROM {table}";
        var countBase = $"SELECT COUNT(1) FROM {table}";
        var keyPredicate = $"{keyColumn} = {prefix}{keyParam}";

        var insertColumns = mapping.InsertableColumns;
        var insertColumnList = string.Join(", ", insertColumns.Select(c => _dialect.QuoteIdentifier(c.ColumnName)));
        var insertValueList = string.Join(", ", insertColumns.Select(c => $"{prefix}{c.PropertyName}"));
        var insert = $"INSERT INTO {table} ({insertColumnList}) VALUES ({insertValueList})";

        var insertReturnsKey = mapping.Key.IsDatabaseGenerated;
        if (insertReturnsKey)
            insert = _dialect.AppendReturningKey(insert, keyColumn);

        var setList = string.Join(", ",
            mapping.UpdatableColumns.Select(c => $"{_dialect.QuoteIdentifier(c.ColumnName)} = {prefix}{c.PropertyName}"));
        var update = $"UPDATE {table} SET {setList} WHERE {keyPredicate}";

        return new EntitySql
        {
            SelectBase = selectBase,
            CountBase = countBase,
            SelectById = $"{selectBase} WHERE {keyPredicate}",
            Insert = insert,
            InsertReturnsKey = insertReturnsKey,
            Update = update,
            DeleteById = $"DELETE FROM {table} WHERE {keyPredicate}",
            KeyColumn = keyColumn,
            KeyParameterName = keyParam,
        };
    }

    private string SelectColumn(ColumnMapping column)
    {
        var quotedColumn = _dialect.QuoteIdentifier(column.ColumnName);
        return column.ColumnName == column.PropertyName
            ? quotedColumn
            : $"{quotedColumn} AS {_dialect.QuoteIdentifier(column.PropertyName)}";
    }

    private string QuoteTable(EntityMapping mapping)
        => mapping.Schema is null
            ? _dialect.QuoteIdentifier(mapping.TableName)
            : $"{_dialect.QuoteIdentifier(mapping.Schema)}.{_dialect.QuoteIdentifier(mapping.TableName)}";
}
