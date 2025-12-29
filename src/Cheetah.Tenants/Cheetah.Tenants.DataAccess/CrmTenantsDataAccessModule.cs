using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Application;
using Microsoft.Extensions.DependencyInjection;


namespace Cheetah.Tenants.DataAccess;

[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class CrmTenantsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddApplicationDbContext<TenantsDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<TenantsDbContext>();
        });
    }
}
