using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;

namespace Cheetah.Modules.Teams.Domain;

[DependsOn(typeof(CoreModule),typeof(CrmDomainModule),typeof(CrmSpecificationModule))]
public class CheetahTeamsDomainModule : CrmModule
{
}