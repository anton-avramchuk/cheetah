using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.Events.InMemory;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmBackendEventsInMemoryModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
