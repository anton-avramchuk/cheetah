using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.StateMachine;

[DependsOn(typeof(CoreModule))]
public class CrmStateMachineModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddStateMachine();
    }
}
