using AppName.Application;
using Cheetah.Core.Modularity;

namespace AppName.Api;

[DependsOn(typeof(AppNameApplicationModule))]
public partial class AppNameApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
