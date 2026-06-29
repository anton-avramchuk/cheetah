using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Teams.Application.EventHandlers.Identity;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain;
using Cheetah.Modules.Teams.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Teams.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Teams: generic CQRS команд + конкретные хендлеры справочников
/// ролей и участников (регистрируются генератором по <c>[Export]</c>). Закрытые generic-handler'ы
/// команды регистрирует наследник через <c>AddTeamsApplication&lt;…&gt;()</c>.
/// <para>
/// Реплика участников из Identity поддерживается двумя путями: фоновый bulk-синк (старт + таймер) для
/// сверки/восстановления и реактивные хендлеры событий Identity (создание/переименование/удаление) —
/// для мгновенного обновления. Оба пути идемпотентны (апсёрт по Id + сравнение по хэшу).
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmGridModule),
    typeof(CheetahTeamsDomainModule),
    typeof(CheetahTeamsContractsModule),
    typeof(CheetahTeamsDomainEventsModule),
    typeof(CheetahIdentityDomainEventsModule))]
public partial class CheetahTeamsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();

        // Реактивное поддержание реплики участников из Identity (в дополнение к фоновому bulk-синку)
        eventBus.Subscribe<UserCreatedEvent, UserCreatedMemberHandler>();
        eventBus.Subscribe<UserNameChangedEvent, UserNameChangedMemberHandler>();
        eventBus.Subscribe<UserDeletedEvent, UserDeletedMemberHandler>();
    }
}
