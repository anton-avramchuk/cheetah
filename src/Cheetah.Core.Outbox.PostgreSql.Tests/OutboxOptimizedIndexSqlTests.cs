using Cheetah.Core.Outbox.PostgreSql;
using Shouldly;

namespace Cheetah.Core.Outbox.PostgreSql.Tests;

public class OutboxOptimizedIndexSqlTests
{
    [Fact]
    public void Create_возвращает_partial_covered_индекс_и_дропает_базовый()
    {
        var sql = OutboxOptimizedIndexSql.Create();

        sql.ShouldContain("DROP INDEX IF EXISTS \"IX_OutboxMessages_Pending\"");
        sql.ShouldContain("CREATE INDEX IF NOT EXISTS \"IX_OutboxMessages_Pending_Partial\"");
        sql.ShouldContain("ON \"OutboxMessages\" (\"OccurredAt\")");
        sql.ShouldContain("INCLUDE (\"Id\", \"EventType\", \"Payload\")");
        sql.ShouldContain("WHERE \"ProcessedAt\" IS NULL");
    }

    [Fact]
    public void Create_без_payload_не_включает_его_в_INCLUDE()
    {
        var sql = OutboxOptimizedIndexSql.Create(includePayload: false);

        sql.ShouldContain("INCLUDE (\"Id\", \"EventType\")");
        sql.ShouldNotContain("Payload");
    }

    [Fact]
    public void Drop_удаляет_оптимизированный_индекс()
    {
        OutboxOptimizedIndexSql.Drop().ShouldContain("DROP INDEX IF EXISTS \"IX_OutboxMessages_Pending_Partial\"");
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("t;DROP TABLE")]
    public void Create_отвергает_небезопасные_имена_таблицы(string table)
    {
        Should.Throw<ArgumentException>(() => OutboxOptimizedIndexSql.Create(table));
    }
}
