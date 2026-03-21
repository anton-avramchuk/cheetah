using AppName.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
#if (databaseType == "postgres")
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
#else
using Cheetah.Core.EntityFramework.MsSql;
using Cheetah.Core.EntityFramework.MsSql.Extensions;
#endif
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace AppName.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(AppNameDomainModule),
    typeof(CrmEntityFrameworkModule),
#if (databaseType == "postgres")
    typeof(CrmEntityFrameworkPostgreSqlModule),
#else
    typeof(CrmEntityFrameworkMsSqlModule),
#endif
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
#if (databaseType == "postgres")
            options.UseNpgsql<AppNameDbContext>();
#else
            options.UseSqlServer<AppNameDbContext>();
#endif
        });
    }
}
