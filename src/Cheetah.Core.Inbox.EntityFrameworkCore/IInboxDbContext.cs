using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

/// <summary>
/// Маркер DbContext'a с таблицей InboxMessages для идемпотентной обработки событий.
/// </summary>
public interface IInboxDbContext
{
    DbSet<InboxMessage> InboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
