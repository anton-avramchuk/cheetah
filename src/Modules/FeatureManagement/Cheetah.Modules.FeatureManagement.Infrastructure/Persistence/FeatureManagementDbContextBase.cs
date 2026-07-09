using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Inbox;
using Cheetah.Core.Inbox.EntityFrameworkCore;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля FeatureManagement. Хостит флаги
/// (<typeparamref name="TFlag"/>) и их детей (правила/варианты/override) в одной БД. Наследник
/// закрывает его конкретным типом флага и поставляет конфигурацию через
/// <see cref="CreateFlagConfiguration"/>. Миграции — у наследника / в <c>.Default</c>.
/// <para>
/// Реализует <see cref="IOutboxDbContext"/> + <see cref="IDeadLetterDbContext"/>: интеграционные
/// события флагов, опубликованные через OutboxEventBus, ложатся в OutboxMessages в той же
/// транзакции, что и сохранение агрегата. Также <see cref="IAuditDbContext"/>: AuditEntries
/// (кто/когда изменил флаг) пишутся AuditInterceptor'ом в том же SaveChanges. И
/// <see cref="IInboxDbContext"/>: InboxMessages — идемпотентная обработка входящих событий
/// (см. <see cref="IdempotentFeatureCacheInvalidator{TContext,TEvent}"/>).
/// </para>
/// </summary>
[ConnectionStringName(FeatureManagementConstants.ConnectionStringName)]
public abstract class FeatureManagementDbContextBase<TContext, TFlag> : CrmDbContext<TContext>,
    IOutboxDbContext, IDeadLetterDbContext, IAuditDbContext, IInboxDbContext
    where TContext : DbContext
    where TFlag : FeatureFlagBase
{
    public DbSet<TFlag> FeatureFlags => Set<TFlag>();
    public DbSet<TargetingRule> TargetingRules => Set<TargetingRule>();
    public DbSet<FeatureVariantDef> FeatureVariants => Set<FeatureVariantDef>();
    public DbSet<TenantOverride> TenantOverrides => Set<TenantOverride>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected FeatureManagementDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateFlagConfiguration());
        modelBuilder.ApplyConfiguration(new TargetingRuleConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureVariantDefConfiguration());
        modelBuilder.ApplyConfiguration(new TenantOverrideConfiguration());

        modelBuilder.AddOutbox();
        modelBuilder.AddDeadLetter();
        modelBuilder.AddAudit();
        modelBuilder.AddInbox();
    }

    /// <summary>Конкретная конфигурация сущности флага, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TFlag> CreateFlagConfiguration();
}
