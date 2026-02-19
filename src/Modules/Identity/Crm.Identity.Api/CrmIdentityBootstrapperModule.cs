using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Events.Redis;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Scalar;
using Crm.Identity.Application;
using Crm.Identity.DataAccess;

namespace Crm.Identity.Api;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmIdentityApplicationModule),
    typeof(CrmIdentityDataAccessModule)
)]
[Bootstrapper]
public partial class CrmIdentityBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}