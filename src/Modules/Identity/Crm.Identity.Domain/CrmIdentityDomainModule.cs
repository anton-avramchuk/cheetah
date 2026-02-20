using Cheetah.Core.Domain;
using Cheetah.Core.Identity.Domain;
using Cheetah.Core.Modularity;
using Crm.Identity.DomainEvents;

namespace Crm.Identity.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmIdentityDomainEventsModule))]
[DependsOn(typeof(Cheetah.Core.Events.CrmEventsCoreModule))]
[DependsOn(typeof(CrmIdentityCoreDomainModule))]
public partial class CrmIdentityDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}