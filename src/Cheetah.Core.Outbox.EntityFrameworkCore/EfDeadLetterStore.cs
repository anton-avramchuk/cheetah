using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

public interface IDeadLetterDbContext
{
    DbSet<DeadLetterMessage> DeadLetterMessages { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class EfDeadLetterStore<TContext> : IDeadLetterStore
    where TContext : DbContext, IDeadLetterDbContext
{
    private readonly TContext _context;

    public EfDeadLetterStore(TContext context) => _context = context;

    public async ValueTask MoveFromOutboxAsync(OutboxMessage source, string lastError, CancellationToken cancellationToken = default)
    {
        var dead = new DeadLetterMessage
        {
            Id = source.Id,
            EventType = source.EventType,
            Payload = source.Payload,
            OccurredAt = source.OccurredAt,
            RetryCount = source.RetryCount + 1,
            LastError = Truncate(lastError, 4000),
            MovedToDeadLetterAt = DateTimeOffset.UtcNow
        };

        _context.DeadLetterMessages.Add(dead);

        // Удаляем из outbox в той же транзакции SaveChanges.
        var existing = await _context.OutboxMessages.FindAsync(new object[] { source.Id }, cancellationToken);
        if (existing is not null)
        {
            _context.OutboxMessages.Remove(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RequeueAsync(Guid deadLetterId, CancellationToken cancellationToken = default)
    {
        var dead = await _context.DeadLetterMessages.FindAsync(new object[] { deadLetterId }, cancellationToken);
        if (dead is null) return;

        var requeued = new OutboxMessage
        {
            Id = dead.Id,
            EventType = dead.EventType,
            Payload = dead.Payload,
            OccurredAt = dead.OccurredAt,
            RetryCount = 0,
            NextAttemptAt = null,
            ProcessedAt = null,
            Error = null
        };
        _context.OutboxMessages.Add(requeued);
        _context.DeadLetterMessages.Remove(dead);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
