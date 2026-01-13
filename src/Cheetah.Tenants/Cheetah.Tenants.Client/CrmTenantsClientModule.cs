using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Tenants.Application;
using Cheetah.Tenants.Contracts;

namespace Cheetah.Tenants.Client;

/// <summary>
/// Client module for accessing Tenant functionality from other modules.
/// This abstraction allows switching between direct access and HTTP client for microservices.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmTenantsContractsModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
public partial class CrmTenantsClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
