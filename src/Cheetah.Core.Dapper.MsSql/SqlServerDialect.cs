using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.MsSql;

/// <summary>
/// SQL Server dialect: <c>[</c>-bracketed identifiers, <c>@</c> parameters,
/// <c>OFFSET/FETCH</c> pagination and an <c>OUTPUT INSERTED</c> clause for generated keys.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISqlDialect))]
public sealed class SqlServerDialect : SqlDialectBase
{
    /// <inheritdoc />
    public override string QuoteIdentifier(string identifier) => $"[{identifier.Replace("]", "]]")}]";

    /// <inheritdoc />
    /// <remarks>SQL Server's <c>OFFSET/FETCH</c> requires an <c>ORDER BY</c>; the supplied
    /// <paramref name="sql"/> must already contain one.</remarks>
    public override string Paginate(string sql, string skipParameterName, string takeParameterName)
        => $"{sql} OFFSET {ParameterPrefix}{skipParameterName} ROWS " +
           $"FETCH NEXT {ParameterPrefix}{takeParameterName} ROWS ONLY";

    /// <inheritdoc />
    public override string AppendReturningKey(string insertSql, string keyColumn)
    {
        const string valuesToken = " VALUES";
        var index = insertSql.IndexOf(valuesToken, StringComparison.Ordinal);
        var output = $" OUTPUT INSERTED.{keyColumn}";

        return index < 0
            ? insertSql + output
            : insertSql.Insert(index, output);
    }
}
