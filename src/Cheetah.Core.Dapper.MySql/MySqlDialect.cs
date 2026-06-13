using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.Dapper.MySql;

/// <summary>
/// MySQL/MariaDB dialect: backtick-quoted identifiers, <c>@</c> parameters,
/// <c>LIMIT/OFFSET</c> pagination and <c>LAST_INSERT_ID()</c> for generated keys.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISqlDialect))]
public sealed class MySqlDialect : SqlDialectBase
{
    /// <inheritdoc />
    public override string QuoteIdentifier(string identifier) => $"`{identifier.Replace("`", "``")}`";

    /// <inheritdoc />
    /// <remarks>MySQL has no <c>RETURNING</c>; the generated auto-increment key is read back
    /// with <c>LAST_INSERT_ID()</c> in the same connection/transaction.</remarks>
    public override string AppendReturningKey(string insertSql, string keyColumn)
        => $"{insertSql}; SELECT LAST_INSERT_ID();";
}
