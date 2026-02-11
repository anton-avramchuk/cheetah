using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.VacancyTasks.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmVacancyTasksDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class CrmVacancyTasksDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<VacancyTasksDbContext>();
        context.Services.AddScoped<VacancyTasksDbContext>();
        context.Services.AddDatabaseMigrator<VacancyTasksDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<VacancyTasksDbContext>(); });
    }
}