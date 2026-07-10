using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Audit.EntityFrameworkCore;

/// <summary>
/// IAuditSink, который добавляет AuditEntry в ChangeTracker текущего DbContext'a.
/// Записи фиксируются в БД той же транзакцией, что и бизнес-агрегат (interceptor
/// вызывает sink ДО SaveChanges).
/// </summary>
public sealed class EfAuditSink<TContext> : IAuditSink
    where TContext : DbContext, IAuditDbContext
{
    private readonly IServiceProvider _serviceProvider;

    /// <remarks>
    /// TContext резолвится лениво, а не через конструктор: sink строится в составе AuditInterceptor,
    /// который сам резолвится при конфигурации DbContextOptions&lt;TContext&gt;. Инъекция контекста
    /// замкнула бы DI в цикл (options -> interceptor -> sink -> context -> options).
    /// </remarks>
    public EfAuditSink(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
    {
        // К моменту вызова (SavingChanges) контекст уже создан и закэширован в scope — тот же экземпляр.
        var context = _serviceProvider.GetRequiredService<TContext>();
        context.AuditEntries.AddRange(entries);
        // SaveChanges не вызываем — interceptor отдаёт нам управление ДО SaveChanges,
        // и наши записи уезжают вместе с агрегатом.
        return ValueTask.CompletedTask;
    }
}
