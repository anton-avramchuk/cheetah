using Cheetah.Core.CQRS;
using Cheetah.Modules.NotesTimeline.Application.Timeline;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.NotesTimeline.Api.Endpoints;

/// <summary>
/// Конкретные read-only эндпоинты ленты (не расширяются полями — гибкость в <c>Payload</c>).
/// Курсорная пагинация: <c>before</c> — отсечка по времени факта, ответ несёт <c>NextCursor</c>.
/// </summary>
public class TimelineEndpoints
{
    protected virtual string RoutePrefix => NotesTimelineConstants.DefaultTimelineRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');
        routes.MapGet(prefix, GetAsync).WithName("GetTimeline").WithTags("Timeline");
    }

    protected virtual async Task<IResult> GetAsync(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromQuery] string? kinds,
        [FromQuery] DateTimeOffset? before,
        [FromQuery] int? limit,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var kindFilter = string.IsNullOrWhiteSpace(kinds)
            ? null
            : kinds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var page = await dispatcher.QueryAsync<GetTimelineQuery, TimelinePageDto>(
            new GetTimelineQuery(entityType, entityId, before, kindFilter, limit), ct);

        return Results.Ok(page);
    }
}
