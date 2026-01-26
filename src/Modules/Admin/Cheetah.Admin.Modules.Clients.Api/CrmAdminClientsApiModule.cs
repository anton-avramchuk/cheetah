using Cheetah.Admin.Modules.Clients.Application;
using Cheetah.AspNetCore;
using Cheetah.Backend.Endpoints;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;

namespace Cheetah.Admin.Modules.Clients.Api;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmAdminClientsApplicationModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmMapsterModule)
    )]
public partial class CrmAdminClientsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}