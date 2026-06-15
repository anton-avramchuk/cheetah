using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendars/{calendarId}/events — создать событие (разовое или серию).</summary>
public sealed class CreateEventEndpoint : CreateCommandEndpoint<CreateEventRequest, CreateEventCommand>
{
    public override string Route => "api/calendars/{calendarId:guid}/events";
    public override string GetByIdRouteName => "GetEvent";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CreateEvent").WithTags("Events");
}

/// <summary>GET api/calendar-events/{eventId} — событие со всеми участниками и напоминаниями.</summary>
public sealed class GetEventByIdEndpoint
    : QueryOrNotFoundEndpoint<GetEventByIdRequest, GetEventByIdQuery, CalendarEventDto, CalendarEventDto>
{
    public override string Route => "api/calendar-events/{eventId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetEvent").WithTags("Events");
}

/// <summary>PATCH api/calendar-events/{eventId} — изменить реквизиты события.</summary>
public sealed class UpdateEventEndpoint : PatchCommandEndpoint<UpdateEventDetailsRequest, UpdateEventDetailsCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("UpdateEvent").WithTags("Events");
}

/// <summary>POST api/calendar-events/{eventId}/reschedule — перенести событие.</summary>
public sealed class RescheduleEventEndpoint : CommandEndpoint<RescheduleEventRequest, RescheduleEventCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/reschedule";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("RescheduleEvent").WithTags("Events");
}

/// <summary>POST api/calendar-events/{eventId}/recurrence — установить/очистить правило повторения.</summary>
public sealed class SetEventRecurrenceEndpoint : CommandEndpoint<SetRecurrenceRequest, SetEventRecurrenceCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/recurrence";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("SetEventRecurrence").WithTags("Events");
}

/// <summary>DELETE api/calendar-events/{eventId} — отменить событие (мягко).</summary>
public sealed class CancelEventEndpoint : DeleteCommandEndpoint<CancelEventRequest, CancelEventCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CancelEvent").WithTags("Events");
}

/// <summary>GET api/calendar-events/by-entity/{entityType}/{entityId} — события сущности в окне.</summary>
public sealed class ListEventsByEntityEndpoint
    : QueryCollectionEndpoint<ListEventsByEntityRequest, ListEventsByEntityQuery, EventOccurrenceDto, EventOccurrenceDto>
{
    public override string Route => "api/calendar-events/by-entity/{entityType}/{entityId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ListEventsByEntity").WithTags("Events");
}

/// <summary>GET api/agenda/{userId} — повестка пользователя в окне.</summary>
public sealed class AgendaEndpoint
    : QueryCollectionEndpoint<ListAgendaRequest, ListAgendaQuery, EventOccurrenceDto, EventOccurrenceDto>
{
    public override string Route => "api/agenda/{userId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetAgenda").WithTags("Events");
}
