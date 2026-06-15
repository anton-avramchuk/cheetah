using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Calendar.Application.Registry;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api.Endpoints;

/// <summary>POST api/calendar/registry/sync — идемпотентная синхронизация реестра привязываемых типов.</summary>
public sealed class SyncRegistryEndpoint : CommandEndpoint<CalendarRegistrySyncRequest, SyncCalendarRegistryCommand>
{
    public override string Route => "api/calendar/registry/sync";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("SyncCalendarRegistry").WithTags("Registry");
}

/// <summary>GET api/calendar/registry — все зарегистрированные привязываемые типы.</summary>
public sealed class GetRegistryEndpoint
    : QueryCollectionEndpoint<GetCalendarableTypesRequest, GetCalendarableTypesQuery, CalendarableEntityTypeDto, CalendarableEntityTypeDto>
{
    public override string Route => "api/calendar/registry";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetCalendarRegistry").WithTags("Registry");
}
