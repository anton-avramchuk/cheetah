using Cheetah.Backend.CQRS;
using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Core.Tenants.Services;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain;
using Cheetah.Features.Events;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Features.Application;

/// <summary>
/// Features Application Module - Contains CQRS handlers and business logic
/// Database migration is handled automatically by TenantDatabaseMigrationManager
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCacheCoreModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmDataAccessModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmFeaturesDomainModule))]
[DependsOn(typeof(CrmFeaturesDataAccessModule))]
[DependsOn(typeof(CrmBackendCQRSModule))]
[DependsOn(typeof(CrmFeaturesEventsModule))]
[DependsOn(typeof(CrmTenantsEventsModule))] // Dependency on Tenants.Events for TenantCreatedEvent
[DependsOn(typeof(CrmTenantsCoreModule))]
public partial class CrmFeaturesApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register module connection string provider for tenant-based database
        // This is automatically used by TenantDatabaseMigrationManager to generate connection strings
        context.Services.AddSingleton<IModuleConnectionStringProvider>(
            new DefaultModuleConnectionStringProvider("Features"));
    }
}
