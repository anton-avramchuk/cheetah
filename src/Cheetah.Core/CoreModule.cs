using Cheetah.Core.Modularity;

namespace Cheetah.Core;

public partial class CoreModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}