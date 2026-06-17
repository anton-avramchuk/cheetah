using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Domain.Abstractions;

/// <summary>
/// Проектор конкретного интеграционного события в строку ленты. Реализуется и регистрируется
/// потребителем (модулем-источником события) — это и есть динамическая расширяемость Timeline:
/// модуль учит ленту новому «виду», не правя NotesTimeline (паттерн Tags-registry).
/// </summary>
public interface ITimelineProjector<in TEvent>
    where TEvent : EventBase
{
    /// <summary>Ключ вида строки (<c>"{service}.{event}"</c>, см. <c>TimelineKinds</c>).</summary>
    string Kind { get; }

    /// <summary>Нужно ли материализовать строку для этого экземпляра события (напр. только DealWon).</summary>
    bool CanProject(TEvent @event);

    /// <summary>Строит строку ленты из события.</summary>
    TimelineEntry Project(TEvent @event);
}
