using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Notification.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Notification.Infrastructure.Persistence;

/// <summary>
/// Реализует <see cref="IOutboxDbContext"/> + <see cref="IDeadLetterDbContext"/>: события,
/// опубликованные через OutboxEventBus, попадают в таблицу OutboxMessages в ТОЙ ЖЕ транзакции,
/// что и бизнес-данные (атомарность «сохранил + опубликовал»). OutboxProcessor релеит их в транспорт.
/// </summary>
[ConnectionStringName(NotificationConstants.ConnectionStringName)]
public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : CrmDbContext<NotificationsDbContext>(options), IOutboxDbContext, IDeadLetterDbContext
{
    public DbSet<NotificationMessage> Notifications => Set<NotificationMessage>();
    public DbSet<NotificationDispatch> Dispatches => Set<NotificationDispatch>();
    public DbSet<RecipientContact> RecipientContacts => Set<RecipientContact>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotificationMessageConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationDispatchConfiguration());
        modelBuilder.ApplyConfiguration(new RecipientContactConfiguration());

        modelBuilder.AddOutbox();      // таблица OutboxMessages
        modelBuilder.AddDeadLetter();  // таблица DeadLetterMessages
    }
}
