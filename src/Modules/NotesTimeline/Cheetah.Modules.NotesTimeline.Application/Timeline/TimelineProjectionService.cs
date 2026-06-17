using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Domain.Abstractions;

namespace Cheetah.Modules.NotesTimeline.Application.Timeline;

/// <summary>
/// Материализует строку ленты из интеграционного события: прогоняет все зарегистрированные для типа
/// события проекторы и пишет результат идемпотентно через <see cref="ITimelineWriter"/>. Вызывается
/// подписчиком шины (Infrastructure) при доставке события-источника; юнит-тестируема без БД/шины.
/// </summary>
public sealed class TimelineProjectionService<TEvent>
    where TEvent : EventBase
{
    private readonly IEnumerable<ITimelineProjector<TEvent>> _projectors;
    private readonly ITimelineWriter _writer;

    public TimelineProjectionService(IEnumerable<ITimelineProjector<TEvent>> projectors, ITimelineWriter writer)
    {
        _projectors = projectors;
        _writer = writer;
    }

    public async ValueTask ProjectAsync(TEvent @event, CancellationToken ct = default)
    {
        foreach (var projector in _projectors)
        {
            if (!projector.CanProject(@event))
                continue;

            var entry = projector.Project(@event);
            await _writer.AppendAsync(entry, ct);
        }
    }
}
