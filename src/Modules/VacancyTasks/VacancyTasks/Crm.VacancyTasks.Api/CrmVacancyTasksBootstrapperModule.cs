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
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.DataAccess;

namespace Crm.VacancyTasks.Api;

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
    typeof(CrmVacancyTasksApplicationModule),
    typeof(CrmVacancyTasksDataAccessModule)
)]
[Bootstrapper]
public partial class CrmVacancyTasksBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}