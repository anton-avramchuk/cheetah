using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Teams.Application;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain;

namespace Cheetah.Modules.Teams.Mapster;

[DependsOn(typeof(CoreModule), typeof(CrmMapsterModule), typeof(CheetahTeamsContractsModule),
    typeof(CheetahTeamsApplicationModule), typeof(CheetahTeamsDomainModule))]
public partial class CheetahTeamsMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
