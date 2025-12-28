using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.CQRS;

[DependsOn(typeof(CrmCQRSCoreModule))]
public partial class CrmBackendCQRSModule:CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}