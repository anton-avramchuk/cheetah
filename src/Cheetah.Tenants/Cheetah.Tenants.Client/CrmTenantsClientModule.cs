using Cheetah.Core.Modularity;
using Cheetah.Tenants.Application;

namespace Cheetah.Tenants.Client;

/// <summary>
/// Client module for accessing Tenant functionality from other modules.
/// This abstraction allows switching between direct access and HTTP client for microservices.
/// </summary>
[DependsOn(typeof(CrmTenantsApplicationModule))]
public partial class CrmTenantsClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
