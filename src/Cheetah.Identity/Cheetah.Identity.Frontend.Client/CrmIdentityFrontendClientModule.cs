using Cheetah.Core.Modularity;
using Cheetah.Identity.Client;

namespace Cheetah.Identity.Frontend.Client;

/// <summary>
/// Identity Frontend Client module - CQRS handlers for Blazor application
/// </summary>
[DependsOn(typeof(CrmIdentityClientModule))]
public partial class CrmIdentityFrontendClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
