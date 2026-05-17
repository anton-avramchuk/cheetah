using Cheetah.Core.Outbox.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Core.Outbox.Integration.Tests;

[Collection("Postgres")]
public class OutboxOptimizedIndexIntegrationTests
{
    private readonly PostgresFixture _fx;

    public OutboxOptimizedIndexIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task Optimized_Index_Is_Created_And_Base_Is_Dropped()
    {
        await using var db = _fx.CreateDbContext();

        await db.Database.ExecuteSqlRawAsync(OutboxOptimizedIndexSql.Create());

        // Проверяем системный каталог: партиальный индекс существует, базовый удалён.
        var partialExists = await IndexExists(db, OutboxOptimizedIndexSql.IndexName);
        var baseExists = await IndexExists(db, OutboxOptimizedIndexSql.DefaultBaseIndexName);

        partialExists.ShouldBeTrue();
        baseExists.ShouldBeFalse();

        // Откатим, чтобы не мешать другим тестам класса.
        await db.Database.ExecuteSqlRawAsync(OutboxOptimizedIndexSql.Drop());
    }

    private static async Task<bool> IndexExists(TestDbContext db, string indexName)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*)::int FROM pg_indexes WHERE indexname = @n";
            var p = cmd.CreateParameter();
            p.ParameterName = "n";
            p.Value = indexName;
            cmd.Parameters.Add(p);
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
