using Cheetah.AspNetCore;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;

namespace Cheetah.Scalar;

[DependsOn(typeof(CrmAspNetCoreModule),typeof(OpenApiModule))]
public class ScalarModule: CrmModule
{
}