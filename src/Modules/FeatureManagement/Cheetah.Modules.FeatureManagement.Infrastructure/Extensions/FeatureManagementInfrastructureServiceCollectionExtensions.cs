using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Extensions;

public static class FeatureManagementInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует БД-инфраструктуру конкретной реализации FeatureManagement: DbContext (флаги +
    /// дети), мигратор, провайдер PostgreSQL, EF-репозиторий флага, реализацию порта
    /// <see cref="IFeatureDefinitionProvider"/> (БД+кэш) и инвалидатор кэша.
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
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });

        // Child-aware репозиторий; оба интерфейса резолвятся в один экземпляр (один DbContext-scope).
        services.AddScoped<FeatureFlagRepository<TContext, TFlag>>();
        services.AddScoped<IFeatureFlagRepository<TFlag>>(sp => sp.GetRequiredService<FeatureFlagRepository<TContext, TFlag>>());
        services.AddScoped<IRepository<TFlag, Guid>>(sp => sp.GetRequiredService<FeatureFlagRepository<TContext, TFlag>>());

        services.AddScoped<IFeatureDefinitionProvider, CachedFeatureDefinitionProvider<TFlag>>();
        services.AddScoped<FeatureCacheInvalidator>();
        return services;
    }
}
