using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.DomainEvents;

namespace Cheetah.Modules.Teams.Domain;

[DependsOn(typeof(CoreModule),
    typeof(CrmDomainModule),
    typeof(CrmSpecificationModule),
    typeof(CheetahTeamsDomainEventsModule))]
public class CheetahTeamsDomainModule : CrmModule
{
}
