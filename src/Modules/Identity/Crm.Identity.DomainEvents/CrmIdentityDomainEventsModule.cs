using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Crm.Identity.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CrmIdentityDomainEventsModule : CrmModule
{
}