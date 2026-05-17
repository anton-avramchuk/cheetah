using Cheetah.Core.Outbox.PostgreSql;
using Shouldly;

namespace Cheetah.Core.Outbox.PostgreSql.Tests;

public class OutboxNotifyTriggerSqlTests
{
    [Fact]
    public void Create_Contains_PgNotify_And_Channel_Name()
    {
        var sql = OutboxNotifyTriggerSql.Create("outbox_new", "OutboxMessages");

        sql.ShouldContain("pg_notify('outbox_new'");
        sql.ShouldContain("AFTER INSERT ON \"OutboxMessages\"");
        sql.ShouldContain("CREATE TRIGGER \"OutboxMessages_notify_trg\"");
    }

    [Fact]
    public void Drop_Removes_Trigger_And_Function()
    {
        var sql = OutboxNotifyTriggerSql.Drop();

        sql.ShouldContain("DROP TRIGGER IF EXISTS \"OutboxMessages_notify_trg\"");
        sql.ShouldContain("DROP FUNCTION IF EXISTS \"OutboxMessages_notify\"()");
    }

    [Theory]
    [InlineData("bad name")]
    [InlineData("name;DROP")]
    [InlineData("'injection'")]
    public void Create_Rejects_Unsafe_Channel_Names(string channel)
    {
        Should.Throw<ArgumentException>(() => OutboxNotifyTriggerSql.Create(channel));
    }
}
