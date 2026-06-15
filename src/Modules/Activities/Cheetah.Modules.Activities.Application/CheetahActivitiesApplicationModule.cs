using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain;
using Cheetah.Modules.Activities.DomainEvents;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Activities: generic CQRS активностей + конечный автомат статуса.
/// Конфигурация автомата (<see cref="ActivityStatus"/>) не зависит от конкретного типа активности и
/// живёт здесь; закрытые generic-handler'ы регистрирует наследник через
/// <c>AddActivitiesApplication&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmStateMachineModule),
    typeof(CheetahActivitiesDomainModule),
    typeof(CheetahActivitiesContractsModule),
    typeof(CheetahActivitiesDomainEventsModule))]
public partial class CheetahActivitiesApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Конечный автомат статуса активности.
        services.AddStateMachine<ActivityStatus>(sm => sm
            .From(ActivityStatus.Open).To(ActivityStatus.InProgress, ActivityStatus.Done, ActivityStatus.Canceled)
            .From(ActivityStatus.InProgress).To(ActivityStatus.Done, ActivityStatus.Canceled)
            .From(ActivityStatus.Done).To(ActivityStatus.Open)
            .From(ActivityStatus.Canceled).To(ActivityStatus.Open));

        RegisterServices(services);
    }
}
