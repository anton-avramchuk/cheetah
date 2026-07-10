using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Audit.Tests;

/// <summary>
/// Аудит подключается к DbContext'у так: конфигурация DbContextOptions&lt;TContext&gt; резолвит
/// AuditInterceptor и передаёт его в AddInterceptors. Значит построение AuditInterceptor (а с ним
/// и его IAuditSink'ов) обязано обходиться БЕЗ TContext — иначе получается цикл
/// DbContextOptions -&gt; AuditInterceptor -&gt; EfAuditSink -&gt; TContext -&gt; DbContextOptions,
/// который DI не диагностирует: StackGuard уводит рекурсию на новые потоки, и процесс просто виснет.
/// </summary>
public class EfAuditSinkResolutionTests
{
    private static ServiceProvider BuildProvider(Action<IServiceCollection> registerContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuditUserAccessor>(NullAuditUserAccessor.Instance);
        services.AddScoped<AuditInterceptor>();
        services.AddEfAuditSink<TestAuditDbContext>();
        registerContext(services);
        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void Resolving_AuditInterceptor_Does_Not_Construct_The_DbContext()
    {
        using var sp = BuildProvider(services => services.AddScoped<TestAuditDbContext>(_ =>
            throw new InvalidOperationException("DbContext был сконструирован при резолве AuditInterceptor")));

        using var scope = sp.CreateScope();

        var interceptor = Should.NotThrow(() => scope.ServiceProvider.GetRequiredService<AuditInterceptor>());
        interceptor.ShouldNotBeNull();
    }

    [Fact]
    public async Task EmitAsync_Adds_Entries_To_The_Scoped_DbContext()
    {
        using var sp = BuildProvider(services => services.AddScoped(_ =>
            new TestAuditDbContext(new DbContextOptionsBuilder<TestAuditDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options)));

        using var scope = sp.CreateScope();
        var sink = scope.ServiceProvider.GetServices<IAuditSink>().OfType<EfAuditSink<TestAuditDbContext>>().Single();

        var entry = new AuditEntry
        {
            EntityType = "T",
            EntityId = "1",
            Action = AuditAction.Created,
            Changes = "{}",
            OccurredAt = DateTimeOffset.UtcNow
        };
        await sink.EmitAsync([entry]);

        // Sink пишет в тот же экземпляр контекста, что живёт в scope — записи уедут одной транзакцией.
        var context = scope.ServiceProvider.GetRequiredService<TestAuditDbContext>();
        context.ChangeTracker.Entries<AuditEntry>().Select(e => e.Entity).ShouldContain(entry);
    }
}
