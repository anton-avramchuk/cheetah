using Cheetah.AspNetCore.Contracts;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Shared;

namespace Cheetah.Tenants.Contracts;

/// <summary>
/// Tenants Contracts module - Requests, ViewModels
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreContractsModule))]
[DependsOn(typeof(CrmTenantsSharedModule))]
public partial class CrmTenantsContractsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
