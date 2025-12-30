using Cheetah.Core.Modularity;
using Cheetah.Identity.Shared;

namespace Cheetah.Identity.Client;

/// <summary>
/// Identity Client module - Backend client for Identity API
/// </summary>
[DependsOn(typeof(CrmIdentitySharedModule))]
public partial class CrmIdentityClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
