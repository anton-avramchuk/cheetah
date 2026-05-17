using System.Reflection;
using Cheetah.Core.Events;

namespace Cheetah.Saga;

/// <summary>
/// Реестр зарегистрированных типов саг и того, на какие события они реагируют.
/// Заполняется при вызове services.AddSaga&lt;TSaga&gt;().
/// </summary>
public sealed class SagaRegistry
{
    private readonly List<SagaRegistration> _registrations = new();

    public IReadOnlyList<SagaRegistration> Registrations => _registrations;

    public void Register(Type sagaType)
    {
        if (!typeof(ISaga).IsAssignableFrom(sagaType))
            throw new ArgumentException($"{sagaType.FullName} does not implement ISaga", nameof(sagaType));

        var startedBy = sagaType.GetCustomAttributes<SagaStartedByAttribute>()
            .Select(a => a.EventType).ToHashSet();
        var handles = sagaType.GetCustomAttributes<SagaHandlesAttribute>()
            .Select(a => a.EventType).ToHashSet();

        // Всё что StartedBy — автоматически и Handles.
        foreach (var t in startedBy) handles.Add(t);

        if (handles.Count == 0)
        {
            throw new InvalidOperationException(
                $"Saga {sagaType.FullName} has no [SagaStartedBy] or [SagaHandles] attributes.");
        }

        _registrations.Add(new SagaRegistration(sagaType, startedBy, handles));
    }

    public IEnumerable<SagaRegistration> ForEvent(Type eventType)
        => _registrations.Where(r => r.HandledEvents.Contains(eventType));
}

public sealed record SagaRegistration(
    Type SagaType,
    IReadOnlySet<Type> StartedByEvents,
    IReadOnlySet<Type> HandledEvents)
{
    public bool CanStart(Type eventType) => StartedByEvents.Contains(eventType);
}
