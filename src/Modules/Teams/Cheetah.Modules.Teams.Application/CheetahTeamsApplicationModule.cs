using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain;
using Cheetah.Modules.Teams.DomainEvents;

namespace Cheetah.Modules.Teams.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Teams: generic CQRS команд + конкретные хендлеры справочников
/// ролей и участников (регистрируются генератором по <c>[Export]</c>). Закрытые generic-handler'ы
/// команды регистрирует наследник через <c>AddTeamsApplication&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmGridModule),
    typeof(CheetahTeamsDomainModule),
    typeof(CheetahTeamsContractsModule),
    typeof(CheetahTeamsDomainEventsModule))]
public partial class CheetahTeamsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
