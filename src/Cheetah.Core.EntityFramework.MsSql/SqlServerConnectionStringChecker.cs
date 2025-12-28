using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.MsSql;

[Export(LifetimeType.Transient, typeof(IConnectionStringChecker))]
public class SqlServerConnectionStringChecker(ILogger<SqlServerConnectionStringChecker> logger)
    : IConnectionStringChecker
{
    private readonly ILogger<SqlServerConnectionStringChecker> _logger = logger;

    public async Task<CrmConnectionStringCheckResult> CheckAsync(string connectionString)
    {
        var result = new CrmConnectionStringCheckResult();
        var connString = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 1
        };

        var oldDatabaseName = connString.InitialCatalog;
        connString.InitialCatalog = "master";

        try
        {
            await using var conn = new SqlConnection(connString.ConnectionString);
            await conn.OpenAsync();
            result.Connected = true;
            await conn.ChangeDatabaseAsync(oldDatabaseName);
            result.DatabaseExists = true;

            await conn.CloseAsync();

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return result;
        }
    }
}