using System.Text.Json;
using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Audit.Integration.Tests;

[Collection("Audit")]
public class EfAuditSinkIntegrationTests
{
    private readonly AuditFixture _fx;

    public EfAuditSinkIntegrationTests(AuditFixture fx) => _fx = fx;

    private AuditDbContext CreateCtxWithInterceptor()
    {
        AuditDbContext? sinkCtx = null;
        var sink = new InlineSink(entries =>
        {
            foreach (var e in entries) sinkCtx!.AuditEntries.Add(e);
        });
        var interceptor = new AuditInterceptor(new[] { (IAuditSink)sink }, NullAuditUserAccessor.Instance);
        var ctx = _fx.CreateDbContext(interceptor);
        sinkCtx = ctx;
        return ctx;
    }

    /// <summary>
    /// Чистим обе таблицы через raw SQL — иначе ChangeTracker.RemoveRange сам
    /// триггерит interceptor и плодит "Deleted" AuditEntry в cleanup'е.
    /// </summary>
    private async Task ResetAsync()
    {
        await using var ctx = _fx.CreateDbContext(null);
        await ctx.Database.ExecuteSqlRawAsync("TRUNCATE \"AuditEntries\", \"Customers\"");
    }

    [Fact]
    public async Task SaveChanges_создаёт_AuditEntry_атомарно_с_агрегатом()
    {
        await ResetAsync();

        await using var ctx = CreateCtxWithInterceptor();
        var customer = new Customer { Name = "Ann", Email = "a@x" };
        ctx.Customers.Add(customer);
        await ctx.SaveChangesAsync();

        await using var verify = _fx.CreateDbContext(null);
        verify.Customers.Any(x => x.Id == customer.Id).ShouldBeTrue();
        var audit = verify.AuditEntries.Single(x => x.EntityId == customer.Id.ToString());
        audit.Action.ShouldBe(AuditAction.Created);
        audit.EntityType.ShouldContain(nameof(Customer));

        using var doc = JsonDocument.Parse(audit.Changes);
        doc.RootElement.GetProperty("Name").GetProperty("new").GetString().ShouldBe("Ann");
    }

    [Fact]
    public async Task Sensitive_свойство_никогда_не_попадает_в_сохранённый_audit()
    {
        await ResetAsync();

        await using var ctx = CreateCtxWithInterceptor();
        var c = new Customer { Name = "X", PasswordHash = "super-secret-hash-12345" };
        ctx.Customers.Add(c);
        await ctx.SaveChangesAsync();

        await using var verify = _fx.CreateDbContext(null);
        var audit = verify.AuditEntries.Single(e => e.EntityId == c.Id.ToString());
        audit.Changes.ShouldNotContain("super-secret-hash-12345");
        audit.Changes.ShouldContain("***");
    }

    [Fact]
    public async Task Откат_транзакции_не_оставляет_AuditEntry()
    {
        await ResetAsync();

        await using var ctx = CreateCtxWithInterceptor();
        await using var tx = await ctx.Database.BeginTransactionAsync();
        var c = new Customer { Name = "Rollback" };
        ctx.Customers.Add(c);
        await ctx.SaveChangesAsync();
        await tx.RollbackAsync();

        await using var verify = _fx.CreateDbContext(null);
        verify.Customers.Any(x => x.Id == c.Id).ShouldBeFalse();
        verify.AuditEntries.Any(e => e.EntityId == c.Id.ToString()).ShouldBeFalse();
    }

    [Fact]
    public async Task Update_сохраняет_old_и_new_в_changes()
    {
        await ResetAsync();

        var customer = new Customer { Name = "Initial", Email = "i@x" };
        await using (var ctx = CreateCtxWithInterceptor())
        {
            ctx.Customers.Add(customer);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateCtxWithInterceptor())
        {
            var loaded = await ctx.Customers.SingleAsync(c => c.Id == customer.Id);
            loaded.Name = "Updated";
            await ctx.SaveChangesAsync();
        }

        await using var verify = _fx.CreateDbContext(null);
        var entries = verify.AuditEntries.Where(e => e.EntityId == customer.Id.ToString()).OrderBy(e => e.OccurredAt).ToList();
        entries.Count.ShouldBe(2);
        entries[0].Action.ShouldBe(AuditAction.Created);
        entries[1].Action.ShouldBe(AuditAction.Updated);

        using var diff = JsonDocument.Parse(entries[1].Changes);
        diff.RootElement.GetProperty("Name").GetProperty("old").GetString().ShouldBe("Initial");
        diff.RootElement.GetProperty("Name").GetProperty("new").GetString().ShouldBe("Updated");
    }

    private sealed class InlineSink : IAuditSink
    {
        private readonly Action<IReadOnlyList<AuditEntry>> _onEmit;
        public InlineSink(Action<IReadOnlyList<AuditEntry>> onEmit) => _onEmit = onEmit;
        public ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
        {
            _onEmit(entries);
            return ValueTask.CompletedTask;
        }
    }
}
