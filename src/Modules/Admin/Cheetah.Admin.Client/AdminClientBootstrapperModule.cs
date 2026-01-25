using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Client;

[DependsOn(typeof(Cheetah.Core.CoreModule))]
[Bootstrapper]

public partial class AdminClientBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
    }
}