using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Sqlite;

[Export(LifetimeType.Transient, typeof(IConnectionStringChecker))]
public class SqliteConnectionStringChecker(ILogger<SqliteConnectionStringChecker> logger)
    : IConnectionStringChecker
{
    public async Task<CrmConnectionStringCheckResult> CheckAsync(string connectionString)
    {
        var result = new CrmConnectionStringCheckResult();

        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var dataSource = builder.DataSource;

            // In-memory БД всегда доступна; файловую считаем существующей, если файл
            // уже есть на диске (открытие соединения создало бы его автоматически).
            var isMemory = builder.Mode == SqliteOpenMode.Memory
                || string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase);
            var existsBeforeOpen = isMemory || File.Exists(dataSource);

            await using var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            result.Connected = true;
            result.DatabaseExists = existsBeforeOpen;
            await conn.CloseAsync();

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return result;
        }
    }
}
