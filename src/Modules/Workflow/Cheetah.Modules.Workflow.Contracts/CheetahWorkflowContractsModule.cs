using Cheetah.Contracts;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CrmEventsCoreModule),
    typeof(CrmWorkflowModule), typeof(CheetahWorkflowSharedModule))]
public class CheetahWorkflowContractsModule : CrmModule
{
}
