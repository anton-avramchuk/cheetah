using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.FeatureManagement.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahFeatureManagementDomainEventsModule : CrmModule
{
}
