using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Deals.DomainEvents;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Domain;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmStateMachineModule))]
[DependsOn(typeof(CheetahDealsSharedModule), typeof(CheetahDealsDomainEventsModule))]
public partial class CheetahDealsDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
