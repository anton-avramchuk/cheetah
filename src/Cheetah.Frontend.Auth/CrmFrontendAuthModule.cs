using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Frontend.Auth;

[DependsOn(typeof(CoreModule))]
public partial class CrmFrontendAuthModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
