using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Application.EventHandlers;
using Cheetah.Tenants.DataAccess;
using Cheetah.Tenants.Domain;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Tenants.Application;

[DependsOn(typeof(CrmTenantsEventsModule))]
[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmTenantsDataAccessModule))]
public partial class CrmTenantsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(Cheetah.Core.ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();

        // Subscribe to TenantCreatedEvent to automatically create tenant database
        eventBus.Subscribe<TenantCreatedEvent, TenantCreatedEventHandler>();
    }
}
