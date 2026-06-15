using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Attendees;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendar-events/{eventId}/attendees — добавить участника.</summary>
public sealed class AddAttendeeEndpoint : CommandEndpoint<AddAttendeeRequest, AddAttendeeCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/attendees";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("AddAttendee").WithTags("Attendees");
}

/// <summary>POST api/calendar-events/{eventId}/attendees/response — ответить на приглашение (RSVP).</summary>
public sealed class RespondToInviteEndpoint : CommandEndpoint<RespondToInviteRequest, RespondToInviteCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/attendees/response";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("RespondToInvite").WithTags("Attendees");
}
