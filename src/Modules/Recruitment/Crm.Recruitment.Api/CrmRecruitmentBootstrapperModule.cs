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
using Crm.Recruitment.Application;
using Crm.Recruitment.DataAccess;

namespace Crm.Recruitment.Api;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmBackendJwtModule),
    typeof(CrmRecruitmentApplicationModule),
    typeof(CrmRecruitmentDataAccessModule)
)]
[Bootstrapper]
public partial class CrmRecruitmentBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}