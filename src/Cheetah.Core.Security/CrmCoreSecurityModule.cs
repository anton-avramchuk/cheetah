using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Security;

[DependsOn(typeof(CoreModule))]
public partial class CrmCoreSecurityModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}