using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Calendar.Application.Attendees;
using Cheetah.Modules.Calendar.Application.Calendars;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Application.Registry;
using Cheetah.Modules.Calendar.Application.Reminders;
using Cheetah.Modules.Calendar.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Calendar.Api;

[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(Application.CheetahCalendarApplicationModule),
    typeof(Contracts.CheetahCalendarContractsModule))]
public partial class CheetahCalendarApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routes = context.GetRouteBuilder();

        // ── Календари ────────────────────────────────────────────────────────────────────
        routes.MapPost("api/calendars", CreateCalendarAsync).WithName("CreateCalendar").WithTags("Calendars");
        routes.MapGet("api/calendars/{calendarId:guid}", GetCalendarAsync).WithName("GetCalendar").WithTags("Calendars");
        routes.MapGet("api/calendars/{calendarId:guid}/events", ListByCalendarAsync)
            .WithName("ListEventsByCalendar").WithTags("Calendars");

        // ── События ──────────────────────────────────────────────────────────────────────
        routes.MapPost("api/calendars/{calendarId:guid}/events", CreateEventAsync).WithName("CreateEvent").WithTags("Events");
        routes.MapGet("api/calendar-events/{eventId:guid}", GetEventAsync).WithName("GetEvent").WithTags("Events");
        routes.MapPatch("api/calendar-events/{eventId:guid}", UpdateEventAsync).WithName("UpdateEvent").WithTags("Events");
        routes.MapPost("api/calendar-events/{eventId:guid}/reschedule", RescheduleAsync).WithName("RescheduleEvent").WithTags("Events");
        routes.MapPost("api/calendar-events/{eventId:guid}/recurrence", SetRecurrenceAsync).WithName("SetEventRecurrence").WithTags("Events");
        routes.MapDelete("api/calendar-events/{eventId:guid}", CancelEventAsync).WithName("CancelEvent").WithTags("Events");
        routes.MapGet("api/calendar-events/by-entity/{entityType}/{entityId:guid}", ListByEntityAsync)
            .WithName("ListEventsByEntity").WithTags("Events");
        routes.MapGet("api/agenda/{userId:guid}", AgendaAsync).WithName("GetAgenda").WithTags("Events");

        // ── Экземпляры серии ─────────────────────────────────────────────────────────────
        routes.MapPost("api/calendar-events/{eventId:guid}/occurrences/{occurrenceKey}/cancel", CancelOccurrenceAsync)
            .WithName("CancelOccurrence").WithTags("Events");
        routes.MapPost("api/calendar-events/{eventId:guid}/occurrences/{occurrenceKey}/override", OverrideOccurrenceAsync)
            .WithName("OverrideOccurrence").WithTags("Events");

        // ── Участники ────────────────────────────────────────────────────────────────────
        routes.MapPost("api/calendar-events/{eventId:guid}/attendees", AddAttendeeAsync).WithName("AddAttendee").WithTags("Attendees");
        routes.MapPost("api/calendar-events/{eventId:guid}/attendees/response", RespondAsync).WithName("RespondToInvite").WithTags("Attendees");

        // ── Напоминания ──────────────────────────────────────────────────────────────────
        routes.MapPost("api/calendar-events/{eventId:guid}/reminders", AddReminderAsync).WithName("AddReminder").WithTags("Reminders");
        routes.MapDelete("api/calendar-events/{eventId:guid}/reminders/{reminderId:guid}", RemoveReminderAsync)
            .WithName("RemoveReminder").WithTags("Reminders");

        // ── Реестр привязываемых типов ───────────────────────────────────────────────────
        routes.MapPost("api/calendar/registry/sync", SyncRegistryAsync).WithName("SyncCalendarRegistry").WithTags("Registry");
        routes.MapGet("api/calendar/registry", GetRegistryAsync).WithName("GetCalendarRegistry").WithTags("Registry");
    }

    private static async Task<IResult> CreateCalendarAsync(
        [FromBody] CreateCalendarRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateCalendarCommand, Guid>(
            new CreateCalendarCommand(request.Name, request.Type, request.OwnerUserId, request.DefaultTimeZoneId, request.Color), ct);
        return Results.Created($"/api/calendars/{id}", new { id });
    }

    private static async Task<IResult> GetCalendarAsync(
        [FromRoute] Guid calendarId, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetCalendarByIdQuery, CalendarDto?>(new GetCalendarByIdQuery(calendarId), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    private static async Task<IResult> ListByCalendarAsync(
        [FromRoute] Guid calendarId, [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListEventsByCalendarQuery, IReadOnlyList<EventOccurrenceDto>>(
            new ListEventsByCalendarQuery(calendarId, from, to), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> CreateEventAsync(
        [FromRoute] Guid calendarId, [FromBody] CreateEventRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateEventCommand, Guid>(new CreateEventCommand(calendarId, request), ct);
        return Results.Created($"/api/calendar-events/{id}", new { id });
    }

    private static async Task<IResult> GetEventAsync(
        [FromRoute] Guid eventId, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetEventByIdQuery, CalendarEventDto?>(new GetEventByIdQuery(eventId), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    private static async Task<IResult> UpdateEventAsync(
        [FromRoute] Guid eventId, [FromBody] UpdateEventDetailsRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new UpdateEventDetailsCommand(eventId, request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RescheduleAsync(
        [FromRoute] Guid eventId, [FromBody] RescheduleEventRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new RescheduleEventCommand(eventId, request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetRecurrenceAsync(
        [FromRoute] Guid eventId, [FromBody] SetRecurrenceRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SetEventRecurrenceCommand(eventId, request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelEventAsync(
        [FromRoute] Guid eventId, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new CancelEventCommand(eventId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListByEntityAsync(
        [FromRoute] string entityType, [FromRoute] Guid entityId,
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListEventsByEntityQuery, IReadOnlyList<EventOccurrenceDto>>(
            new ListEventsByEntityQuery(entityType, entityId, from, to), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> AgendaAsync(
        [FromRoute] Guid userId, [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListAgendaQuery, IReadOnlyList<EventOccurrenceDto>>(
            new ListAgendaQuery(userId, from, to), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> CancelOccurrenceAsync(
        [FromRoute] Guid eventId, [FromRoute] string occurrenceKey,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new CancelOccurrenceCommand(eventId, occurrenceKey), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> OverrideOccurrenceAsync(
        [FromRoute] Guid eventId, [FromRoute] string occurrenceKey,
        [FromBody] OverrideOccurrenceRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(
            new OverrideOccurrenceCommand(eventId, occurrenceKey, request.NewStartUtc, request.NewEndUtc, request.NewTitle), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddAttendeeAsync(
        [FromRoute] Guid eventId, [FromBody] AddAttendeeRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new AddAttendeeCommand(eventId, request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RespondAsync(
        [FromRoute] Guid eventId, [FromBody] RespondToInviteRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new RespondToInviteCommand(eventId, request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddReminderAsync(
        [FromRoute] Guid eventId, [FromBody] AddReminderRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<AddReminderCommand, Guid>(new AddReminderCommand(eventId, request), ct);
        return Results.Created($"/api/calendar-events/{eventId}/reminders/{id}", new { id });
    }

    private static async Task<IResult> RemoveReminderAsync(
        [FromRoute] Guid eventId, [FromRoute] Guid reminderId,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new RemoveReminderCommand(eventId, reminderId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SyncRegistryAsync(
        [FromBody] CalendarRegistrySyncRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SyncCalendarRegistryCommand(request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetRegistryAsync([FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<GetCalendarableTypesQuery, IReadOnlyList<CalendarableEntityTypeDto>>(
            new GetCalendarableTypesQuery(), ct);
        return Results.Ok(items);
    }
}
