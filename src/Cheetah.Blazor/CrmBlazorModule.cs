using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Blazor;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
