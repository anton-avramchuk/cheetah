using Cheetah.Mapping.Core;
using Cheetah.Modules.Calendar.Application.Attendees;
using Cheetah.Modules.Calendar.Application.Calendars;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Application.Registry;
using Cheetah.Modules.Calendar.Application.Reminders;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Calendar (аналог Mapster-профиля, но через source generator).
/// Маркер <see cref="GenerateMapperAttribute"/> размещён здесь, в выделенной маппинг-сборке, поэтому
/// Contracts остаётся чистым (без ссылки на Application). Все маппинги — Request → Command/Query,
/// проекция не нужна (<c>GenerateProjection = false</c>).
/// </summary>
// Календари
[GenerateMapper(typeof(CreateCalendarRequest), typeof(CreateCalendarCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetCalendarByIdRequest), typeof(GetCalendarByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ListEventsByCalendarRequest), typeof(ListEventsByCalendarQuery), GenerateProjection = false)]
// События
[GenerateMapper(typeof(CreateEventRequest), typeof(CreateEventCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetEventByIdRequest), typeof(GetEventByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(UpdateEventDetailsRequest), typeof(UpdateEventDetailsCommand), GenerateProjection = false)]
[GenerateMapper(typeof(RescheduleEventRequest), typeof(RescheduleEventCommand), GenerateProjection = false)]
[GenerateMapper(typeof(SetRecurrenceRequest), typeof(SetEventRecurrenceCommand), GenerateProjection = false)]
[GenerateMapper(typeof(CancelEventRequest), typeof(CancelEventCommand), GenerateProjection = false)]
[GenerateMapper(typeof(ListEventsByEntityRequest), typeof(ListEventsByEntityQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ListAgendaRequest), typeof(ListAgendaQuery), GenerateProjection = false)]
[GenerateMapper(typeof(GetUserBusyRequest), typeof(GetUserBusyQuery), GenerateProjection = false)]
// Экземпляры серии
[GenerateMapper(typeof(CancelOccurrenceRequest), typeof(CancelOccurrenceCommand), GenerateProjection = false)]
[GenerateMapper(typeof(OverrideOccurrenceRequest), typeof(OverrideOccurrenceCommand), GenerateProjection = false)]
// Участники
[GenerateMapper(typeof(AddAttendeeRequest), typeof(AddAttendeeCommand), GenerateProjection = false)]
[GenerateMapper(typeof(RespondToInviteRequest), typeof(RespondToInviteCommand), GenerateProjection = false)]
// Напоминания
[GenerateMapper(typeof(AddReminderRequest), typeof(AddReminderCommand), GenerateProjection = false)]
[GenerateMapper(typeof(RemoveReminderRequest), typeof(RemoveReminderCommand), GenerateProjection = false)]
// Реестр
[GenerateMapper(typeof(CalendarRegistrySyncRequest), typeof(SyncCalendarRegistryCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetCalendarableTypesRequest), typeof(GetCalendarableTypesQuery), GenerateProjection = false)]
public static partial class CalendarMappingRegistry
{
}
