using Cheetah.AspNetCore;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Events.Redis;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Scalar;
using Crm.Recruitment.Application;

namespace Crm.Recruitment.Api;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmRecruitmentApplicationModule)
)]
[Bootstrapper]
public partial class CrmRecruitmentBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}