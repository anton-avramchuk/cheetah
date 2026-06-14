using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Modules.Email.Domain.Entities;
using Cheetah.Modules.Email.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Email.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Email.Infrastructure.Persistence;

/// <summary>
/// Реализует <see cref="IOutboxDbContext"/> + <see cref="IDeadLetterDbContext"/>: обратные статусы
/// (EmailDelivered/EmailFailed), опубликованные через OutboxEventBus, попадают в OutboxMessages
/// в той же транзакции, что и маркер SentEmail. OutboxProcessor релеит их в транспорт.
/// </summary>
[ConnectionStringName(EmailConstants.ConnectionStringName)]
public class EmailDbContext(DbContextOptions<EmailDbContext> options)
    : CrmDbContext<EmailDbContext>(options), IOutboxDbContext, IDeadLetterDbContext
{
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SentEmailConfiguration());

        modelBuilder.AddOutbox();      // таблица OutboxMessages
        modelBuilder.AddDeadLetter();  // таблица DeadLetterMessages
    }
}
