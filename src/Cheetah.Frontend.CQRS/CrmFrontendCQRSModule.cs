using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Frontend.CQRS;

[DependsOn(typeof(CrmCQRSCoreModule))]
public partial class CrmFrontendCQRSModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}