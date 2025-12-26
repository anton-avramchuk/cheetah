using Cheetah.AspNetCore;
using Cheetah.Core.Modules;
using Cheetah.OpenApi;

namespace Cheetah.Scalar;

[DependsOn(typeof(CrmAspNetCoreModule),typeof(OpenApiModule))]
public class ScalarModule: CrmModule
{
}