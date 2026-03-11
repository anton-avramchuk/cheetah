using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.Customer.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Customer.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCustomerDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule)
)]
public partial class CrmCustomerDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<CustomerDbContext>();
        context.Services.AddScoped<CustomerDbContext>();
        context.Services.AddDatabaseMigrator<CustomerDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<CustomerDbContext>(); });
    }
}