using Cheetah.AspNetCore;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Scalar;

namespace Cheetah.Admin.Api;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmAspNetCoreModule), typeof(ScalarModule))]
[Bootstrapper]
public partial class AdminApiBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}