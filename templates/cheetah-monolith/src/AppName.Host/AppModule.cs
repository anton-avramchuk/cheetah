using AppName.Api;
using AppName.DataAccess;
using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Events.Redis;
using Cheetah.Backend.Jwt;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Scalar;
#if (includeIdentity)
using AppName.Identity.DataAccess;
using Cheetah.Modules.Identity.Api;
#endif

namespace AppName.Host;

[DependsOn(
    typeof(CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmBackendJwtModule),
#if (includeIdentity)
    typeof(CheetahIdentityApiModule),
    typeof(AppNameIdentityDataAccessModule),
#endif
    typeof(AppNameApiModule),
    typeof(AppNameDataAccessModule)
)]
[Bootstrapper]
public partial class AppModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
