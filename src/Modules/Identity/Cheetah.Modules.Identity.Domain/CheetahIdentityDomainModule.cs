using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DomainEvents;

namespace Cheetah.Modules.Identity.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule))]
[DependsOn(typeof(CheetahIdentityDomainEventsModule))]
public partial class CheetahIdentityDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
