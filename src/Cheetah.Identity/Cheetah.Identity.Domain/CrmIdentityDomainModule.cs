using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Domain;

/// <summary>
/// Identity Domain Module
/// </summary>
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmIdentityEventsModule))]
public partial class CrmIdentityDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
