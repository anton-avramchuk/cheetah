namespace Cheetah.Core.Dapper.Dialect;

/// <summary>
/// Encapsulates the SQL syntax differences between database engines
/// (identifier quoting, parameter prefix, pagination, identity retrieval).
/// Implemented per provider package.
/// </summary>
public interface ISqlDialect
{
    /// <summary>The character used to prefix named parameters (e.g. <c>@</c> or <c>:</c>).</summary>
    char ParameterPrefix { get; }

    /// <summary>Quotes a single identifier (table or column name).</summary>
    /// <param name="identifier">The raw identifier.</param>
    string QuoteIdentifier(string identifier);

    /// <summary>
    /// Appends pagination to a <c>SELECT</c> statement.
    /// </summary>
    /// <param name="sql">The base SELECT statement (without trailing semicolon).</param>
    /// <param name="skipParameterName">Name of the parameter holding the offset.</param>
    /// <param name="takeParameterName">Name of the parameter holding the row count.</param>
    string Paginate(string sql, string skipParameterName, string takeParameterName);

    /// <summary>
    /// Builds an <c>INSERT</c> statement that also returns the generated key value
    /// (e.g. <c>RETURNING</c> in PostgreSQL, <c>OUTPUT</c>/<c>SELECT SCOPE_IDENTITY()</c> in SQL Server).
    /// </summary>
    /// <param name="insertSql">The base INSERT statement.</param>
    /// <param name="keyColumn">The already-quoted key column.</param>
    string AppendReturningKey(string insertSql, string keyColumn);
}
