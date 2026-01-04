using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.EntityFramework.Tenants.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Domain;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Tenants.DataAccess;

[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkTenantsModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class CrmTenantsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddTenantsDbContext<TenantsDbContext, Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>();

        Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<TenantsDbContext>();
        });

        // Register database migrator for automatic migrations
        context.Services.AddDatabaseMigrator<TenantsDbContext>();
    }
}
