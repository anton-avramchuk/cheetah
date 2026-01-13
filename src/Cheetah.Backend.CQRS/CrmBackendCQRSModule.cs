using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.CQRS;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
public partial class CrmBackendCQRSModule:CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}