using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Client;
using Cheetah.Modules.Teams.Domain;
using Cheetah.Modules.Teams.Infrastructure.Identity;
using Cheetah.Modules.Teams.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Modules.Teams.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Teams: абстрактные базы EF
/// (<see cref="Persistence.TeamsDbContextBase{TContext,TTeam}"/>,
/// <see cref="Persistence.Configurations.TeamConfigurationBase{TTeam}"/>) и generic-регистрация через
/// <c>AddTeamsInfrastructure&lt;TContext,TTeam&gt;()</c>. Конкретный DbContext, конфигурацию команды и
/// миграции создаёт наследник.
/// <para>
/// Здесь же — интеграция с Identity: адаптер
/// <see cref="Identity.IdentityClientUserDirectory"/> ([Export]) и фоновый
/// <see cref="Identity.TeamMemberDirectorySyncService"/>, который поддерживает реплику участников
/// в актуальном состоянии (синк по хэшу, без лишних записей в БД).
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CheetahTeamsDomainModule),
    typeof(CheetahTeamsSharedModule),
    typeof(CheetahIdentityClientModule))]
public partial class CheetahTeamsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services); // в т.ч. адаптер IIdentityUserDirectory ([Export])

        services.AddOptions<TeamMemberSyncOptions>()
            .Bind(services.GetConfiguration().GetSection(TeamMemberSyncOptions.SectionName));

        // Bulk-синк реплики участников из Identity. Регистрируется после мигратора наследника
        // (AddTeamsInfrastructure) на уровне хоста, поэтому к старту схема уже готова.
        services.AddHostedService<TeamMemberDirectorySyncService>();
    }
}
