using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Events.Redis;
using Cheetah.Backend.Jwt;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Api.Middleware;
using Cheetah.Modules.Identity.Application;
using Cheetah.Modules.Identity.Contracts;
using Crm.Identity.DataAccess;
using Cheetah.Scalar;

namespace Cheetah.Modules.Identity.Api;

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
    typeof(CheetahIdentityApplicationModule),
    typeof(CheetahIdentityContractsModule),
    typeof(CrmIdentityDataAccessModule)
)]
[Bootstrapper]
public partial class CheetahIdentityApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddExceptionHandler<InvalidCredentialsExceptionHandler>();
    }
}
