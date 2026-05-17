using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Audit.Tests;

public class AuditInterceptorTests
{
    private static TestAuditDbContext CreateContext(AuditInterceptor interceptor)
    {
        var opts = new DbContextOptionsBuilder<TestAuditDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        return new TestAuditDbContext(opts);
    }

    [Fact]
    public async Task Auditable_сущности_создают_AuditEntry_NonAuditable_нет()
    {
        TestAuditDbContext? captureCtx = null;
        var sink = new CaptureSink();
        var interceptor = new AuditInterceptor(new[] { (IAuditSink)sink }, NullAuditUserAccessor.Instance);

        using var ctx = CreateContext(interceptor);
        captureCtx = ctx;
        sink.InterceptedSinkCalled = entries =>
        {
            // EfAuditSink писал бы в БД, но в этом тесте мы только проверяем что собрано.
            foreach (var e in entries)
                captureCtx!.AuditEntries.Add(e);
        };

        ctx.Customers.Add(new TestCustomer { Name = "Ann" });
        ctx.Others.Add(new TestNonAudited { Title = "skipme" });
        await ctx.SaveChangesAsync();

        ctx.AuditEntries.Count().ShouldBe(1);
        var entry = ctx.AuditEntries.First();
        entry.EntityType.ShouldContain(nameof(TestCustomer));
        entry.Action.ShouldBe(AuditAction.Created);
    }

    [Fact]
    public async Task User_context_попадает_в_AuditEntry()
    {
        var user = new StubUser("u-123", "Anton", "tenant-x", "corr-y");
        var sink = new CaptureSink();
        var interceptor = new AuditInterceptor(new[] { (IAuditSink)sink }, user);

        using var ctx = CreateContext(interceptor);
        sink.InterceptedSinkCalled = entries =>
        {
            foreach (var e in entries) ctx.AuditEntries.Add(e);
        };

        ctx.Customers.Add(new TestCustomer { Name = "X" });
        await ctx.SaveChangesAsync();

        var entry = ctx.AuditEntries.Single();
        entry.UserId.ShouldBe("u-123");
        entry.UserName.ShouldBe("Anton");
        entry.TenantId.ShouldBe("tenant-x");
        entry.CorrelationId.ShouldBe("corr-y");
    }

    [Fact]
    public async Task Multiple_операции_в_одном_SaveChanges_дают_несколько_AuditEntry()
    {
        var sink = new CaptureSink();
        var interceptor = new AuditInterceptor(new[] { (IAuditSink)sink }, NullAuditUserAccessor.Instance);

        using var ctx = CreateContext(interceptor);
        sink.InterceptedSinkCalled = entries =>
        {
            foreach (var e in entries) ctx.AuditEntries.Add(e);
        };

        var c1 = new TestCustomer { Name = "A" };
        var c2 = new TestCustomer { Name = "B" };
        ctx.Customers.AddRange(c1, c2);
        await ctx.SaveChangesAsync();

        ctx.AuditEntries.Count().ShouldBe(2);
    }

    private sealed class CaptureSink : IAuditSink
    {
        public Action<IReadOnlyList<AuditEntry>>? InterceptedSinkCalled;

        public ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
        {
            InterceptedSinkCalled?.Invoke(entries);
            return ValueTask.CompletedTask;
        }
    }

    private sealed record StubUser(string? UserId, string? UserName, string? TenantId, string? CorrelationId) : IAuditUserAccessor;
}
