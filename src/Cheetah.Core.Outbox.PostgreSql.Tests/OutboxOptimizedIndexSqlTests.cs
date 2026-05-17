using Cheetah.Core.Outbox.PostgreSql;
using Shouldly;

namespace Cheetah.Core.Outbox.PostgreSql.Tests;

public class OutboxOptimizedIndexSqlTests
{
    [Fact]
    public void Create_Returns_Partial_Covered_Index_And_Drops_Base()
    {
        var sql = OutboxOptimizedIndexSql.Create();

        sql.ShouldContain("DROP INDEX IF EXISTS \"IX_OutboxMessages_Pending\"");
        sql.ShouldContain("CREATE INDEX IF NOT EXISTS \"IX_OutboxMessages_Pending_Partial\"");
        sql.ShouldContain("ON \"OutboxMessages\" (\"OccurredAt\")");
        sql.ShouldContain("INCLUDE (\"Id\", \"EventType\", \"Payload\")");
        sql.ShouldContain("WHERE \"ProcessedAt\" IS NULL");
    }

    [Fact]
    public void Create_Without_Payload_Excludes_It_From_INCLUDE()
    {
        var sql = OutboxOptimizedIndexSql.Create(includePayload: false);

        sql.ShouldContain("INCLUDE (\"Id\", \"EventType\")");
        sql.ShouldNotContain("Payload");
    }

    [Fact]
    public void Drop_Removes_Optimized_Index()
    {
        OutboxOptimizedIndexSql.Drop().ShouldContain("DROP INDEX IF EXISTS \"IX_OutboxMessages_Pending_Partial\"");
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("t;DROP TABLE")]
    public void Create_Rejects_Unsafe_Table_Names(string table)
    {
        Should.Throw<ArgumentException>(() => OutboxOptimizedIndexSql.Create(table));
    }
}
