using Cheetah.AspNetCore;
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
    typeof(CoreModule)
)]
public partial class BootstrapperModule : CrmModule
{
}