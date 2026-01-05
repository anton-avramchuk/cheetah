using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Events.Redis;
using Cheetah.Core;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.EntityFramework.Tenants.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.OpenApi;
using Cheetah.Scalar;
using Cheetah.Tenants.Api;
using Cheetah.Tenants.Events;

namespace Cheetah.Crm;

[Bootstrapper]
[DependsOn(
    typeof(CrmAspNetCoreModule),
    typeof(OpenApiModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CoreModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmTenantsApiModule),
    typeof(CrmTenantsEventsModule),
    typeof(CrmEntityFrameworkTenantsModule)
)]
public partial class BootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register TenantDatabaseMigrationManager as singleton
        // It will automatically subscribe to TenantCreatedEvent
        context.Services.AddTenantDatabaseMigrationManager<TenantCreatedEvent>();
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await base.OnApplicationInitializationAsync(context);
        
        context.GetApplicationBuilder().MigrateTenantDatabases<TenantCreatedEvent>();
    }
}