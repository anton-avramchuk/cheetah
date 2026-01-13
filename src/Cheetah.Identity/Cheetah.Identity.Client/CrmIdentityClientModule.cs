using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Contracts;

namespace Cheetah.Identity.Client;

/// <summary>
/// Identity Client module - Backend client for Identity API
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmIdentityContractsModule))]
public partial class CrmIdentityClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
