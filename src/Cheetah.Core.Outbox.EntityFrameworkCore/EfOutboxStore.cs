using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

/// <summary>
/// EF Core реализация IOutboxStore поверх конкретного DbContext'a, реализующего IOutboxDbContext.
/// Регистрируется через services.AddOutboxStore&lt;TContext&gt;().
/// </summary>
public sealed class EfOutboxStore<TContext> : IOutboxStore
    where TContext : DbContext, IOutboxDbContext
{
    private readonly TContext _context;

    public EfOutboxStore(TContext context) => _context = context;

    public ValueTask AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.OutboxMessages.Add(message);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var list = await _context.OutboxMessages
            .Where(x => x.ProcessedAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async ValueTask MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }
        entity.ProcessedAt = DateTimeOffset.UtcNow;
        entity.Error = null;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }
        entity.RetryCount += 1;
        entity.Error = Truncate(error, 4000);
        entity.NextAttemptAt = nextAttemptAt;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
