using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace AppName.Core;

[DependsOn(typeof(CoreModule))]
public partial class AppNameCoreModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
