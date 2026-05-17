using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.EntityFrameworkCore;

/// <summary>
/// IAuditSink, который добавляет AuditEntry в ChangeTracker текущего DbContext'a.
/// Записи фиксируются в БД той же транзакцией, что и бизнес-агрегат (interceptor
/// вызывает sink ДО SaveChanges).
/// </summary>
public sealed class EfAuditSink<TContext> : IAuditSink
    where TContext : DbContext, IAuditDbContext
{
    private readonly TContext _context;

    public EfAuditSink(TContext context) => _context = context;

    public ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
    {
        _context.AuditEntries.AddRange(entries);
        // SaveChanges не вызываем — interceptor отдаёт нам управление ДО SaveChanges,
        // и наши записи уезжают вместе с агрегатом.
        return ValueTask.CompletedTask;
    }
}
