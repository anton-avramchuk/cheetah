using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.DataAccess;
using Cheetah.Modules.Identity.DataAccess.Extensions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Identity.DataAccess;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CheetahIdentityDomainModule),
    typeof(CrmIdentityCoreDataAccessModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class CrmIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddIdentityContext<IdentityModuleDbContext, CrmIdentityUser, CrmIdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
        });

        context.Services.AddScoped<IdentityModuleDbContext>();
        context.Services.AddDatabaseMigrator<IdentityModuleDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<IdentityModuleDbContext>(); });
    }
}
