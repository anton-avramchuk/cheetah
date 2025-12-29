using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Domain;
using Microsoft.Extensions.DependencyInjection;


namespace Cheetah.Tenants.DataAccess;

[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class CrmTenantsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddApplicationDbContext<TenantsDbContext>();

        Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<TenantsDbContext>();
        });

        
        // Register database migrator for automatic migrations
        context.Services.AddDatabaseMigrator<TenantsDbContext>();
    }
}
