using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.DataAccess;

[DependsOn(typeof(CoreModule),typeof(CrmEntityFrameworkPostgreSqlModule))]
[DependsOn(typeof(CrmEntityFrameworkModule),typeof(CrmAdminClientsDomainModule))]
[DependsOn(typeof(Cheetah.Core.Grid.CrmGridModule))]

public partial class CrmAdminClientsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddApplicationDbContext<ClientsDbContext>();
        context.Services.AddScoped<ClientsDbContext>();
        context.Services.AddDatabaseMigrator<ClientsDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<ClientsDbContext>();
        });
    }
}