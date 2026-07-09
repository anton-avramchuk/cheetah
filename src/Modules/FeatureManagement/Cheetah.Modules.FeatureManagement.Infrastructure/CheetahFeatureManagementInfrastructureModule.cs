using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Core.Inbox;
using Cheetah.Core.Inbox.EntityFrameworkCore;
using Cheetah.Core.Inbox.PostgreSql;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Core.Outbox.PostgreSql;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain;
using Cheetah.Modules.FeatureManagement.DomainEvents;

namespace Cheetah.Modules.FeatureManagement.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля FeatureManagement: абстрактные базы EF
/// (<see cref="Persistence.FeatureManagementDbContextBase{TContext,TFlag}"/>,
/// <see cref="Persistence.Configurations.FeatureFlagConfigurationBase{TFlag}"/>), реализация порта
/// <c>IFeatureDefinitionProvider</c> (БД+кэш) и инвалидатор. Generic-регистрация —
/// <c>AddFeatureManagementInfrastructure&lt;TContext,TFlag&gt;()</c>. Конкретный DbContext,
/// конфигурацию и миграции создаёт наследник / <c>.Default</c>.
/// <para>
/// Публикация интеграционных событий — через транзакционный Outbox (как в Deals): OutboxEventBus
/// кладёт события в OutboxMessages той же БД, OutboxProcessor релеит их в транспорт. Изменения
/// флагов аудируются (<c>Cheetah.Audit</c>): AuditInterceptor пишет AuditEntries в том же SaveChanges.
/// Обработка входящих событий (инвалидатор кэша) идемпотентна через Inbox
/// (<see cref="IdempotentFeatureCacheInvalidator{TContext,TEvent}"/>): повторная доставка одного
/// и того же события шиной не приводит к повторной инвалидации.
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmCacheCoreModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmOutboxModule),
    typeof(CrmOutboxEntityFrameworkCoreModule),
    typeof(CrmOutboxPostgreSqlModule),
    typeof(CrmInboxModule),
    typeof(CrmInboxEntityFrameworkCoreModule),
    typeof(CrmInboxPostgreSqlModule),
    typeof(CrmAuditModule),
    typeof(CrmAuditEntityFrameworkCoreModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementDomainModule),
    typeof(CheetahFeatureManagementDomainEventsModule))]
public partial class CheetahFeatureManagementInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
