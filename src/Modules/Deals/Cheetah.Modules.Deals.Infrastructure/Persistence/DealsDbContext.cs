using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Deals.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Deals.Infrastructure.Persistence;

/// <summary>
/// БД модуля Deals. Реализует <see cref="IOutboxDbContext"/> + <see cref="IDeadLetterDbContext"/>:
/// интеграционные события сделок, опубликованные через OutboxEventBus, ложатся в OutboxMessages
/// в той же транзакции, что и сохранение агрегата.
/// </summary>
[ConnectionStringName(DealsConstants.ConnectionStringName)]
public class DealsDbContext(DbContextOptions<DealsDbContext> options)
    : CrmDbContext<DealsDbContext>(options), IOutboxDbContext, IDeadLetterDbContext
{
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<DealStageHistory> DealStageHistory => Set<DealStageHistory>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PipelineConfiguration());
        modelBuilder.ApplyConfiguration(new PipelineStageConfiguration());
        modelBuilder.ApplyConfiguration(new DealConfiguration());
        modelBuilder.ApplyConfiguration(new DealStageHistoryConfiguration());

        modelBuilder.AddOutbox();
        modelBuilder.AddDeadLetter();
    }
}
