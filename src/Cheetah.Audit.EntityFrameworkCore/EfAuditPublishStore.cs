using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.EntityFrameworkCore;

/// <summary>
/// IAuditPublishStore поверх EF Core. Для Postgres-нагрузки рекомендуется наследник
/// с SELECT FOR UPDATE SKIP LOCKED — см. PostgresAuditPublishStore в Cheetah.Audit.Kafka.
/// </summary>
public class EfAuditPublishStore<TContext> : IAuditPublishStore
    where TContext : DbContext, IAuditDbContext
{
    protected readonly TContext Context;

    public EfAuditPublishStore(TContext context) => Context = context;

    public virtual async ValueTask<IReadOnlyList<AuditEntry>> ClaimPendingAsync(int batchSize, TimeSpan claimTimeout, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var claimUntil = now + claimTimeout;

        var pending = await Context.AuditEntries
            .Where(x => x.PublishedAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0) return pending;

        // Claim: сдвигаем NextAttemptAt вперёд, чтобы другая реплика не забрала.
        // ВНИМАНИЕ: без SKIP LOCKED это race-prone. Production-вариант — PostgresAuditPublishStore.
        var ids = pending.Select(p => p.Id).ToHashSet();
        await Context.AuditEntries
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.NextAttemptAt, _ => claimUntil), cancellationToken);

        return pending;
    }

    public async ValueTask MarkPublishedAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return;
        var now = DateTimeOffset.UtcNow;
        var idArray = ids as Guid[] ?? ids.ToArray();

        await Context.AuditEntries
            .Where(x => idArray.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.PublishedAt, _ => now)
                .SetProperty(x => x.Error, _ => null), cancellationToken);
    }

    public async ValueTask MarkFailedAsync(Guid id, string error, DateTimeOffset nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var entity = await Context.AuditEntries.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null) return;

        entity.RetryCount += 1;
        entity.Error = Truncate(error, 4000);
        entity.NextAttemptAt = nextAttemptAt;
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
