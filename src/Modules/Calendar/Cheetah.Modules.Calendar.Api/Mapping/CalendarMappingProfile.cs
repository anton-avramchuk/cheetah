using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Calendar.Application.Attendees;
using Cheetah.Modules.Calendar.Application.Calendars;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Application.Registry;
using Cheetah.Modules.Calendar.Application.Reminders;
using Cheetah.Modules.Calendar.Contracts;
using Mapster;

namespace Cheetah.Modules.Calendar.Api.Mapping;

/// <summary>
/// Маппинг HTTP-реквестов в CQRS-команды/запросы. Имена полей реквестов и команд совпадают —
/// Mapster соединяет их по соглашению; конфиги ниже регистрируют пары явно. Ответы (DTO) совпадают
/// с результатами обработчиков по типу, поэтому отдельных правил не требуют.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class CalendarMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Календари
        config.NewConfig<CreateCalendarRequest, CreateCalendarCommand>();
        config.NewConfig<GetCalendarByIdRequest, GetCalendarByIdQuery>();
        config.NewConfig<ListEventsByCalendarRequest, ListEventsByCalendarQuery>();

        // События
        config.NewConfig<CreateEventRequest, CreateEventCommand>();
        config.NewConfig<GetEventByIdRequest, GetEventByIdQuery>();
        config.NewConfig<UpdateEventDetailsRequest, UpdateEventDetailsCommand>();
        config.NewConfig<RescheduleEventRequest, RescheduleEventCommand>();
        config.NewConfig<SetRecurrenceRequest, SetEventRecurrenceCommand>();
        config.NewConfig<CancelEventRequest, CancelEventCommand>();
        config.NewConfig<ListEventsByEntityRequest, ListEventsByEntityQuery>();
        config.NewConfig<ListAgendaRequest, ListAgendaQuery>();

        // Экземпляры серии
        config.NewConfig<CancelOccurrenceRequest, CancelOccurrenceCommand>();
        config.NewConfig<OverrideOccurrenceRequest, OverrideOccurrenceCommand>();

        // Участники
        config.NewConfig<AddAttendeeRequest, AddAttendeeCommand>();
        config.NewConfig<RespondToInviteRequest, RespondToInviteCommand>();

        // Напоминания
        config.NewConfig<AddReminderRequest, AddReminderCommand>();
        config.NewConfig<RemoveReminderRequest, RemoveReminderCommand>();

        // Реестр
        config.NewConfig<CalendarRegistrySyncRequest, SyncCalendarRegistryCommand>();
        config.NewConfig<GetCalendarableTypesRequest, GetCalendarableTypesQuery>();
    }
}
