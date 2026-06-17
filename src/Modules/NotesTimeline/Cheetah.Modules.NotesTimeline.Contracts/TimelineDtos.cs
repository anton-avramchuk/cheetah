using System.Text.Json;
using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.NotesTimeline.Contracts;

/// <summary>
/// Строка ленты (граница API). Конкретный (не расширяемый полями) DTO: гибкость хронологии — в
/// произвольной нагрузке <see cref="Payload"/> (jsonb), а не в новых колонках.
/// </summary>
public sealed record TimelineEntryDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string Kind { get; init; } = null!;
    public string Title { get; init; } = null!;
    public JsonElement? Payload { get; init; }
    public Guid? ActorId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

/// <summary>Курсорная страница ленты: элементы + курсор следующей страницы (null — конец).</summary>
public sealed record TimelinePageDto(IReadOnlyList<TimelineEntryDto> Items, DateTimeOffset? NextCursor);

/// <summary>
/// Дескриптор «вида» строки ленты для регистрации в каталоге потребителем (как TagsTypeDescriptor).
/// </summary>
public sealed record TimelineKindDescriptor(string Kind, string OwnerService, string TitleTemplate);
