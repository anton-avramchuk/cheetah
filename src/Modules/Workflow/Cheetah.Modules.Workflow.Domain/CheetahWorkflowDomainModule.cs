using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmWorkflowModule))]
[DependsOn(typeof(CheetahWorkflowSharedModule), typeof(CheetahWorkflowDomainEventsModule))]
public partial class CheetahWorkflowDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
