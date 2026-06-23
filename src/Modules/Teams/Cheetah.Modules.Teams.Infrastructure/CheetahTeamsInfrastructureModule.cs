using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Teams.Domain;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Teams: абстрактные базы EF
/// (<see cref="Persistence.TeamsDbContextBase{TContext,TTeam}"/>,
/// <see cref="Persistence.Configurations.TeamConfigurationBase{TTeam}"/>) и generic-регистрация через
/// <c>AddTeamsInfrastructure&lt;TContext,TTeam&gt;()</c>. Конкретный DbContext, конфигурацию команды и
/// миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CheetahTeamsDomainModule),
    typeof(CheetahTeamsSharedModule))]
public partial class CheetahTeamsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
