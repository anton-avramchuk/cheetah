using System.Text.Json;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Domain.Specifications;
using Cheetah.Modules.NotesTimeline.Shared;

namespace Cheetah.Modules.NotesTimeline.Application.Timeline;

/// <summary>
/// Курсорная страница ленты сущности: строки старше <paramref name="Before"/> (null — с самых свежих),
/// опционально отфильтрованные по видам, по убыванию времени факта.
/// </summary>
public sealed record GetTimelineQuery(
    string EntityType, Guid EntityId, DateTimeOffset? Before, IReadOnlyCollection<string>? Kinds, int? Limit)
    : IQuery<TimelinePageDto>;

public class GetTimelineQueryHandler : IQueryHandler<GetTimelineQuery, TimelinePageDto>
{
    private readonly IRepository<TimelineEntry, Guid> _repository;

    public GetTimelineQueryHandler(IRepository<TimelineEntry, Guid> repository) => _repository = repository;

    public async ValueTask<TimelinePageDto> HandleAsync(GetTimelineQuery query, CancellationToken ct = default)
    {
        var limit = Math.Clamp(
            query.Limit ?? NotesTimelineConstants.DefaultTimelinePageSize,
            1, NotesTimelineConstants.MaxTimelinePageSize);

        var spec = new TimelineByEntitySpecification(query.EntityType, query.EntityId, query.Before, query.Kinds);
        var all = await _repository.GetAllAsync(spec, ct);

        var page = all
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.Id)
            .Take(limit)
            .ToArray();

        var items = page.Select(Map).ToArray();
        var nextCursor = page.Length == limit ? page[^1].OccurredAt : (DateTimeOffset?)null;

        return new TimelinePageDto(items, nextCursor);
    }

    private static TimelineEntryDto Map(TimelineEntry e) => new()
    {
        Id = e.Id,
        EntityType = e.EntityType,
        EntityId = e.EntityId,
        Kind = e.Kind,
        Title = e.Title,
        Payload = ParsePayload(e.Payload),
        ActorId = e.ActorId,
        OccurredAt = e.OccurredAt
    };

    private static JsonElement? ParsePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        using var doc = JsonDocument.Parse(payload);
        return doc.RootElement.Clone();
    }
}
