using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Candidates.DataAccess;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCandidatesDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule)
)]
public partial class CrmCandidatesDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddApplicationDbContext<CandidatesDbContext>();
        context.Services.AddScoped<CandidatesDbContext>();
        context.Services.AddDatabaseMigrator<CandidatesDbContext>();

        context.Services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<CandidatesDbContext>(); });
    }
}