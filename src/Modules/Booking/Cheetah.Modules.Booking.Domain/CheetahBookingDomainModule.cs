using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Booking.DomainEvents;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Domain;

/// <summary>
/// Доменный слой шаблонного модуля Booking: абстрактные агрегаты (тип встречи, доступность, бронь) +
/// слот-движок (<see cref="Slots.ISlotEngine"/>, регистрируется через <c>[Export]</c>). Конкретные
/// sealed-типы и миграции — у наследника.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmStateMachineModule))]
[DependsOn(typeof(CheetahBookingSharedModule), typeof(CheetahBookingDomainEventsModule))]
public partial class CheetahBookingDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
