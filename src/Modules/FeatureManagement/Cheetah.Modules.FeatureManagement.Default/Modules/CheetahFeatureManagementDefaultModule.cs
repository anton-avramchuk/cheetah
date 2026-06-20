using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application;
using Cheetah.Modules.FeatureManagement.Application.Extensions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Default.Contracts;
using Cheetah.Modules.FeatureManagement.Default.Endpoints;
using Cheetah.Modules.FeatureManagement.Default.Entities;
using Cheetah.Modules.FeatureManagement.Default.Factories;
using Cheetah.Modules.FeatureManagement.Default.Persistence;
using Cheetah.Modules.FeatureManagement.Domain;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Cheetah.Modules.FeatureManagement.Infrastructure;
using Cheetah.Modules.FeatureManagement.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.FeatureManagement.Default.Modules;

/// <summary>
/// Модуль «из коробки»: закрывает шаблон FeatureManagement конкретными типами. В одной сборке ровно
/// один модуль (требование генератора), поэтому инфраструктура, прикладной слой и маппинг эндпоинтов
/// собраны здесь. <c>ConfigureServices</c> регистрирует DbContext/репозитории/провайдер
/// (<c>AddFeatureManagementInfrastructure</c>) и фабрику/проектор/CQRS (<c>AddFeatureManagementApplication</c>);
/// <c>OnApplicationInitialization</c> маппит эндпоинты и подписывает инвалидатор кэша на шину.
/// </summary>
[DependsOn(typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementDomainModule),
    typeof(CheetahFeatureManagementInfrastructureModule),
    typeof(CheetahFeatureManagementApplicationModule),
    typeof(CheetahFeatureManagementContractsModule),
    typeof(CrmAspNetCoreModule))]
public partial class CheetahFeatureManagementDefaultModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddFeatureManagementInfrastructure<FeatureManagementDbContext, FeatureFlag>();

        services.AddFeatureManagementApplication<
            FeatureFlag, CreateFeatureFlagRequest, FeatureFlagDto, FeatureFlagFactory, FeatureFlagProjector>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new FeatureFlagEndpoints().Map(context.GetRouteBuilder());

        // Инвалидация кэша определений по событиям (в кластере — через общую шину).
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<FeatureFlagChangedIntegrationEvent, FeatureCacheInvalidator>();
        eventBus.Subscribe<FeatureFlagToggledIntegrationEvent, FeatureCacheInvalidator>();
    }
}
