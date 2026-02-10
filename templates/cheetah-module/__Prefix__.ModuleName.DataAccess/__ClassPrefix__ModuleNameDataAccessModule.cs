using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using __Prefix__.ModuleName.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace __Prefix__.ModuleName.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(__ClassPrefix__ModuleNameDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule)
)]
public partial class __ClassPrefix__ModuleNameDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<ModuleNameDbContext>();
        context.Services.AddScoped<ModuleNameDbContext>();
        context.Services.AddDatabaseMigrator<ModuleNameDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<ModuleNameDbContext>();
        });
    }
}
