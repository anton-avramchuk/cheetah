using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Identity.Application.EventHandlers;
using Cheetah.Identity.Contracts;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Domain;
using Cheetah.Identity.Events;
using Cheetah.Tenants.Client;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Identity.Application;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmIdentityDomainModule))]
[DependsOn(typeof(CrmIdentityContractsModule))]
[DependsOn(typeof(CrmIdentityDataAccessModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmIdentityEventsModule))]
[DependsOn(typeof(CrmTenantsEventsModule))]
[DependsOn(typeof(CrmTenantsClientModule))]
[DependsOn(typeof(CrmEntityFrameworkTenantsModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
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
