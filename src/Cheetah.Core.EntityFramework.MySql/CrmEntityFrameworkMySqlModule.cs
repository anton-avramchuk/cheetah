using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.EntityFramework.MySql;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class CrmEntityFrameworkMySqlModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}