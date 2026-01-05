using Cheetah.Backend.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Features.Domain;
using Cheetah.Features.Events;
using Cheetah.Tenants.Events;

namespace Cheetah.Features.Application;

/// <summary>
/// Features Application Module - Contains CQRS handlers and business logic
/// Database migration is handled automatically by TenantDatabaseMigrationManager
/// </summary>
[DependsOn(typeof(CrmFeaturesDomainModule))]
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
