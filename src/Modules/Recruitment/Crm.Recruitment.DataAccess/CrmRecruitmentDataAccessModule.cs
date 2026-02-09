using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Recruitment.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmRecruitmentDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule)
)]
public partial class CrmRecruitmentDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<RecruitmentDbContext>();
        context.Services.AddScoped<RecruitmentDbContext>();
        context.Services.AddDatabaseMigrator<RecruitmentDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<RecruitmentDbContext>(); });
    }
}