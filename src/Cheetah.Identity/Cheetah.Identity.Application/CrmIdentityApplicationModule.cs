using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application.EventHandlers;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Domain;
using Cheetah.Identity.Events;
using Cheetah.Tenants.Client;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Identity.Application;

[DependsOn(typeof(CrmIdentityDomainModule))]
[DependsOn(typeof(CrmIdentityDataAccessModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmIdentityEventsModule))]
[DependsOn(typeof(CrmTenantsEventsModule))]
[DependsOn(typeof(CrmTenantsClientModule))]
public partial class CrmIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();

        // Subscribe to TenantCreatedEvent to automatically create Identity database for new tenants
        eventBus.Subscribe<TenantCreatedEvent, TenantCreatedEventHandler>();
    }
}
