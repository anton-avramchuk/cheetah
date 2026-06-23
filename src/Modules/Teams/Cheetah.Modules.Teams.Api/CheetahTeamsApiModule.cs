using Cheetah.AspNetCore;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Teams.Api;

[DependsOn(typeof(CoreModule), typeof(CrmAspNetCoreModule), typeof(CrmCQRSCoreModule))]
public partial class CheetahTeamsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}