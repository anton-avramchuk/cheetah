using AppName.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace AppName.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(AppNameDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule)
)]
public partial class AppNameDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<AppNameDbContext>();
        context.Services.AddScoped<AppNameDbContext>();
        context.Services.AddDatabaseMigrator<AppNameDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<AppNameDbContext>();
        });
    }
}
