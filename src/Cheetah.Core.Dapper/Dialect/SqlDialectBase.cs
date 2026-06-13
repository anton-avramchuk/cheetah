namespace Cheetah.Core.Dapper.Dialect;

/// <summary>
/// Base dialect implementing the most common ANSI-SQL behaviour
/// (<c>"</c>-quoted identifiers, <c>@</c> parameters, <c>LIMIT/OFFSET</c> pagination).
/// Providers override only what differs.
/// </summary>
public abstract class SqlDialectBase : ISqlDialect
{
    /// <inheritdoc />
    public virtual char ParameterPrefix => '@';

    /// <inheritdoc />
    public virtual string QuoteIdentifier(string identifier) => $"\"{identifier}\"";

    /// <inheritdoc />
    public virtual string Paginate(string sql, string skipParameterName, string takeParameterName)
        => $"{sql} LIMIT {ParameterPrefix}{takeParameterName} OFFSET {ParameterPrefix}{skipParameterName}";

    /// <inheritdoc />
    public virtual string AppendReturningKey(string insertSql, string keyColumn)
        => $"{insertSql} RETURNING {keyColumn}";
}
