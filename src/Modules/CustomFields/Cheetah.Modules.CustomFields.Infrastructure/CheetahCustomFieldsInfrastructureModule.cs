using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.CustomFields.Domain;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.CustomFields.Infrastructure;

/// <summary>
/// Инфраструктура CustomFields: EF Core <see cref="CustomFieldsDbContext"/>, миграции, репозитории и
/// cache-aside-чтение определений (<see cref="CachedCustomFieldDefinitionReader"/>) + инвалидатор кэша
/// по событиям изменения определений (подписка на шину в <see cref="OnApplicationInitialization"/>).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCacheCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahCustomFieldsDomainModule))]
public partial class CheetahCustomFieldsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<CustomFieldsDbContext>();
        services.AddScoped<CustomFieldsDbContext>();
        services.AddDatabaseMigrator<CustomFieldsDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<CustomFieldsDbContext>(); });

        services.AddScoped<CustomFieldDefinitionCacheInvalidator>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<CustomFieldDefinitionCreatedIntegrationEvent, CustomFieldDefinitionCacheInvalidator>();
        eventBus.Subscribe<CustomFieldDefinitionChangedIntegrationEvent, CustomFieldDefinitionCacheInvalidator>();
        eventBus.Subscribe<CustomFieldDefinitionDeactivatedIntegrationEvent, CustomFieldDefinitionCacheInvalidator>();
    }
}
