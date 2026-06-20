using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>
/// Клиентский модуль FeatureManagement (server-to-server). Подключается потребителем; сам клиент,
/// реплика и реестр регистрируются через <c>services.AddFeatureManagementClient(...)</c>. Здесь —
/// подписка локальной реплики на события инвалидации (push с шины), если реплика включена.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementContractsModule),
    typeof(CheetahFeatureManagementDomainEventsModule))]
public partial class CrmFeatureManagementClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // Если включена локальная реплика — подписать её на события изменения флагов (push).
        if (context.ServiceProvider.GetService<RemoteFeatureDefinitionProvider>() is null)
            return;

        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<FeatureFlagChangedIntegrationEvent, RemoteFeatureDefinitionProvider>();
        eventBus.Subscribe<FeatureFlagToggledIntegrationEvent, RemoteFeatureDefinitionProvider>();
    }
}
