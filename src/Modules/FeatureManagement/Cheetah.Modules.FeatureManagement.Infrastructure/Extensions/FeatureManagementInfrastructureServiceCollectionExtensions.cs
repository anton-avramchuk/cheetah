using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Events;
using Cheetah.Core.Inbox;
using Cheetah.Core.Inbox.EntityFrameworkCore;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Core.Outbox.PostgreSql;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Extensions;

public static class FeatureManagementInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует БД-инфраструктуру конкретной реализации FeatureManagement: DbContext (флаги +
    /// дети), мигратор, провайдер PostgreSQL, EF-репозиторий флага, реализацию порта
    /// <see cref="IFeatureDefinitionProvider"/> (БД+кэш) и инвалидатор кэша.
    /// Плюс транзакционный Outbox (store + DLQ поверх той же БД) и аудит изменений
    /// (AuditInterceptor + EF-sink: AuditEntries пишутся в том же SaveChanges, что и флаг).
    /// Вызывается из инфраструктурного модуля наследника / <c>.Default</c>.
    /// </summary>
    public static IServiceCollection AddFeatureManagementInfrastructure<TContext, TFlag>(
        this IServiceCollection services)
        where TContext : FeatureManagementDbContextBase<TContext, TFlag>
        where TFlag : FeatureFlagBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options =>
        {
            options.Configure<TContext>(context =>
            {
                context.UseNpgsql();
                // Аудит [Auditable]-сущностей: interceptor собирает diff и отдаёт sink'ам.
                context.DbContextOptions.AddInterceptors(
                    context.ServiceProvider.GetRequiredService<AuditInterceptor>());
            });
        });

        services.AddPostgresOutboxStore<TContext>();
        services.AddDeadLetterStore<TContext>();
        services.AddEfAuditSink<TContext>();
        services.AddInboxStore<TContext>();

        // Child-aware репозиторий; оба интерфейса резолвятся в один экземпляр (один DbContext-scope).
        services.AddScoped<FeatureFlagRepository<TContext, TFlag>>();
        services.AddScoped<IFeatureFlagRepository<TFlag>>(sp => sp.GetRequiredService<FeatureFlagRepository<TContext, TFlag>>());
        services.AddScoped<IRepository<TFlag, Guid>>(sp => sp.GetRequiredService<FeatureFlagRepository<TContext, TFlag>>());

        services.AddScoped<IFeatureDefinitionProvider, CachedFeatureDefinitionProvider<TFlag>>();

        // Инвалидатор кэша + идемпотентная подписка (Inbox): IEventHandler<TEvent> резолвит
        // InboxIdempotentEventHandler<TEvent> инвалидатор как inner, а конкретный generic-тип
        // IdempotentFeatureCacheInvalidator<TContext,TEvent> — то, на что подписывается IEventBus
        // (см. AddFeatureManagementInfrastructure caller / OnApplicationInitialization).
        services.AddScoped<FeatureCacheInvalidator>();
        services.AddScoped<IEventHandler<FeatureFlagChangedIntegrationEvent>>(sp => sp.GetRequiredService<FeatureCacheInvalidator>());
        services.AddScoped<IEventHandler<FeatureFlagToggledIntegrationEvent>>(sp => sp.GetRequiredService<FeatureCacheInvalidator>());
        services.AddScoped<InboxIdempotentEventHandler<FeatureFlagChangedIntegrationEvent>>();
        services.AddScoped<InboxIdempotentEventHandler<FeatureFlagToggledIntegrationEvent>>();
        services.AddScoped<IdempotentFeatureCacheInvalidator<TContext, FeatureFlagChangedIntegrationEvent>>();
        services.AddScoped<IdempotentFeatureCacheInvalidator<TContext, FeatureFlagToggledIntegrationEvent>>();
        return services;
    }
}
