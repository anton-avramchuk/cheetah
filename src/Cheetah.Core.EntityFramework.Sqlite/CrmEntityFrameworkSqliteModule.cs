using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.EntityFramework.Sqlite;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class CrmEntityFrameworkSqliteModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
