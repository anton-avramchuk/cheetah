using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.EntityFramework.MsSql;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class CrmEntityFrameworkMsSqlModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}