using Cheetah.Core.Modularity;
using Cheetah.Identity.Client;
using Cheetah.Identity.Contracts;

namespace Cheetah.Identity.Frontend.Client;

/// <summary>
/// Identity Frontend Client module - CQRS handlers for Blazor application
/// </summary>
[DependsOn(typeof(CrmIdentityClientModule))]
[DependsOn(typeof(CrmIdentityContractsModule))]
public partial class CrmIdentityFrontendClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
