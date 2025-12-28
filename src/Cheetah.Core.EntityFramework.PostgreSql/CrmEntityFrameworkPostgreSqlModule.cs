using Cheetah.Core.Modularity;

namespace Cheetah.Core.EntityFramework.PostgreSql;

[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class CrmEntityFrameworkPostgreSqlModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}