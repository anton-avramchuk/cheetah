using Cheetah.Admin.Modules.Clients.Application;
using Cheetah.Admin.Modules.Clients.DataAccess;
using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;

namespace Cheetah.Admin.Modules.Clients.Api;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmAdminClientsApplicationModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmMapsterModule),
    typeof(CrmAdminClientsDataAccessModule)
    )]
public partial class CrmAdminClientsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}