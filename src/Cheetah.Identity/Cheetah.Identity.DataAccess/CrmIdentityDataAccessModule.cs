using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Identity.Domain;

namespace Cheetah.Identity.DataAccess;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmIdentityDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
[DependsOn(typeof(CrmEntityFrameworkTenantsModule))]
public partial class CrmIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        
        Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<IdentityDbContext>();
        });

    }
}
