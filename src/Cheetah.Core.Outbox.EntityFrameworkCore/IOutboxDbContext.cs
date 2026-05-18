using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

/// <summary>
/// Маркер DbContext'a, который содержит таблицу OutboxMessages.
/// Должен быть реализован пользовательским DbContext'ом модуля.
/// </summary>
public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
