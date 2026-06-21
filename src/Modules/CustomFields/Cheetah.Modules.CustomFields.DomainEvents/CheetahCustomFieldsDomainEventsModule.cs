using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.CustomFields.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahCustomFieldsDomainEventsModule : CrmModule
{
}
