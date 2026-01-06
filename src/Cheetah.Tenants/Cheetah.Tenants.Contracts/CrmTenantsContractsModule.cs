using Cheetah.Core.Modularity;
using Cheetah.Tenants.Shared;

namespace Cheetah.Tenants.Contracts;

/// <summary>
/// Tenants Contracts module - Requests, ViewModels
/// </summary>
[DependsOn(typeof(CrmTenantsSharedModule))]
public partial class CrmTenantsContractsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
