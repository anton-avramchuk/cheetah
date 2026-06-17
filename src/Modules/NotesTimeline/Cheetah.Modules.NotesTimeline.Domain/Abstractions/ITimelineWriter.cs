using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Domain.Abstractions;

/// <summary>
/// Запись строки в ленту хронологии. Порт в Domain; реализация (EF) — в Infrastructure. Append
/// идемпотентен по <see cref="TimelineEntry.SourceEventId"/>: повторная доставка события-источника
/// не создаёт дубль строки.
/// </summary>
public interface ITimelineWriter
{
    /// <summary>Добавляет строку. Возвращает <c>false</c>, если строка с тем же источником уже есть.</summary>
    ValueTask<bool> AppendAsync(TimelineEntry entry, CancellationToken ct = default);
}
