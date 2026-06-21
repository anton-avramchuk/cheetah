using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Workflow.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahWorkflowDomainEventsModule : CrmModule
{
}
