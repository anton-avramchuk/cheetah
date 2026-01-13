using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Shared;

namespace Cheetah.Identity.Contracts;

/// <summary>
/// Identity Contracts module - Requests, ViewModels, Responses
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmIdentitySharedModule))]
public partial class CrmIdentityContractsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
