using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Calendars;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendars — создать календарь.</summary>
public sealed class CreateCalendarEndpoint : CreateCommandEndpoint<CreateCalendarRequest, CreateCalendarCommand>
{
    public override string Route => "api/calendars";
    public override string GetByIdRouteName => "GetCalendar";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CreateCalendar").WithTags("Calendars");
}

/// <summary>GET api/calendars/{calendarId} — календарь по идентификатору.</summary>
public sealed class GetCalendarByIdEndpoint
    : QueryOrNotFoundEndpoint<GetCalendarByIdRequest, GetCalendarByIdQuery, CalendarDto, CalendarDto>
{
    public override string Route => "api/calendars/{calendarId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetCalendar").WithTags("Calendars");
}

/// <summary>GET api/calendars/{calendarId}/events — события календаря в окне.</summary>
public sealed class ListEventsByCalendarEndpoint
    : QueryCollectionEndpoint<ListEventsByCalendarRequest, ListEventsByCalendarQuery, EventOccurrenceDto, EventOccurrenceDto>
{
    public override string Route => "api/calendars/{calendarId:guid}/events";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ListEventsByCalendar").WithTags("Calendars");
}
