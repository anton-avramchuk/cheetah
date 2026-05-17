using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

/// <summary>
/// EF Core реализация IOutboxStore поверх конкретного DbContext'a, реализующего IOutboxDbContext.
/// Регистрируется через services.AddOutboxStore&lt;TContext&gt;().
/// </summary>
public class EfOutboxStore<TContext> : IOutboxStore
    where TContext : DbContext, IOutboxDbContext
{
    protected readonly TContext Context;

    public EfOutboxStore(TContext context) => Context = context;

    public ValueTask AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        Context.OutboxMessages.Add(message);
        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var list = await Context.OutboxMessages
            .Where(x => x.ProcessedAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async ValueTask MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }
        entity.ProcessedAt = DateTimeOffset.UtcNow;
        entity.Error = null;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var entity = await Context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }
        entity.RetryCount += 1;
        entity.Error = Truncate(error, 4000);
        entity.NextAttemptAt = nextAttemptAt;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask<int> DeleteProcessedAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default)
    {
        // ExecuteDelete не поддерживает Take в EF Core у всех провайдеров одинаково,
        // поэтому используем составной запрос: вычислить Id'шники, потом удалить по ним.
        var idsToDelete = await Context.OutboxMessages
            .Where(x => x.ProcessedAt != null && x.ProcessedAt < olderThan)
            .OrderBy(x => x.ProcessedAt)
            .Take(batchSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (idsToDelete.Count == 0)
            return 0;

        return await Context.OutboxMessages
            .Where(x => idsToDelete.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
