using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Activities.DomainEvents;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Domain;

[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmStateMachineModule))]
[DependsOn(typeof(CheetahActivitiesSharedModule), typeof(CheetahActivitiesDomainEventsModule))]
public partial class CheetahActivitiesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
