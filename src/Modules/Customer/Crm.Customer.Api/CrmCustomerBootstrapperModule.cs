using Cheetah.AspNetCore;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Events.Redis;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Scalar;
using Crm.Customer.Application;

namespace Crm.Customer.Api;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmCustomerApplicationModule)
)]
[Bootstrapper]
public partial class CrmCustomerBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}