using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

public sealed class EfInboxStore<TContext> : IInboxStore
    where TContext : DbContext, IInboxDbContext
{
    private readonly TContext _context;

    public EfInboxStore(TContext context) => _context = context;

    public async ValueTask<bool> AlreadyProcessedAsync(Guid eventId, string consumerName, CancellationToken cancellationToken = default)
        => await _context.InboxMessages
            .AnyAsync(x => x.EventId == eventId && x.ConsumerName == consumerName, cancellationToken);

    public ValueTask AddAsync(InboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.InboxMessages.Add(message);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<int> DeleteOlderThanAsync(DateTimeOffset olderThan, int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.InboxMessages
            .Where(x => x.ReceivedAt < olderThan)
            .OrderBy(x => x.ReceivedAt)
            .Take(batchSize)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
