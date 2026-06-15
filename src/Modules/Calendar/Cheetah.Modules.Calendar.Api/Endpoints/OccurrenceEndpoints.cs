using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendar-events/{eventId}/occurrences/{occurrenceKey}/cancel — отменить экземпляр серии.</summary>
public sealed class CancelOccurrenceEndpoint : CommandEndpoint<CancelOccurrenceRequest, CancelOccurrenceCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/occurrences/{occurrenceKey}/cancel";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CancelOccurrence").WithTags("Events");
}

/// <summary>POST api/calendar-events/{eventId}/occurrences/{occurrenceKey}/override — переопределить экземпляр серии.</summary>
public sealed class OverrideOccurrenceEndpoint : CommandEndpoint<OverrideOccurrenceRequest, OverrideOccurrenceCommand>
{
    public override string Route => "api/calendar-events/{eventId:guid}/occurrences/{occurrenceKey}/override";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("OverrideOccurrence").WithTags("Events");
}
