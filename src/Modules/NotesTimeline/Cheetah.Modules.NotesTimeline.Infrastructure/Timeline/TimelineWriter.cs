using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.NotesTimeline.Domain.Abstractions;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Domain.Specifications;

namespace Cheetah.Modules.NotesTimeline.Infrastructure.Timeline;

/// <summary>
/// EF-реализация <see cref="ITimelineWriter"/>. Идемпотентна по <see cref="TimelineEntry.SourceEventId"/>:
/// перед вставкой проверяет наличие строки с тем же источником (двойная защита с уникальным индексом).
/// </summary>
public sealed class TimelineWriter : ITimelineWriter
{
    private readonly IRepository<TimelineEntry, Guid> _repository;

    public TimelineWriter(IRepository<TimelineEntry, Guid> repository) => _repository = repository;

    public async ValueTask<bool> AppendAsync(TimelineEntry entry, CancellationToken ct = default)
    {
        if (entry.SourceEventId is { Length: > 0 } sourceEventId)
        {
            var alreadyExists = await _repository.ExistsAsync(
                new TimelineEntryBySourceEventSpecification(sourceEventId), ct);
            if (alreadyExists)
                return false;
        }

        _repository.Add(entry);
        await _repository.SaveChangesAsync(ct);
        return true;
    }
}
