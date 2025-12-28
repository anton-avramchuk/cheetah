using Cheetah.AspNetCore;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Events.Redis;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.OpenApi;
using Cheetah.Scalar;

namespace Cheetah.Crm;

[Bootstrapper]
[DependsOn(
    typeof(CrmAspNetCoreModule),
    typeof(OpenApiModule),
    typeof(ScalarModule),
    typeof(CrmMapsterModule),
    typeof(CoreModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEventsRedisModule)
)]
public partial class BootstrapperModule : CrmModule
{
}