using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Frontend.Events;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmFrontendEventsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}