using AppName.Identity.Domain;
using Cheetah.Core.EntityFramework;
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
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.DataAccess;
using Cheetah.Modules.Identity.DataAccess.Extensions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AppName.Identity.DataAccess;

[DependsOn(
    typeof(CheetahIdentityDomainModule),
    typeof(AppNameIdentityDomainModule),
    typeof(CrmIdentityCoreDataAccessModule),
#if (databaseType == "postgres")
    typeof(CrmEntityFrameworkPostgreSqlModule),
#else
    typeof(CrmEntityFrameworkMsSqlModule),
#endif
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class AppNameIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddIdentityContext<IdentityDbContext, AppNameIdentityUser, AppNameIdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
        });

        context.Services.AddScoped<IdentityDbContext>();
        context.Services.AddDatabaseMigrator<IdentityDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
#if (databaseType == "postgres")
            options.UseNpgsql<IdentityDbContext>();
#else
            options.UseSqlServer<IdentityDbContext>();
#endif
        });
    }
}
