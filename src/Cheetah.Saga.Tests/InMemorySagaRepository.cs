using Cheetah.Saga;

namespace Cheetah.Saga.Tests;

/// <summary>
/// In-memory реализация ISagaRepository для unit-тестов orchestrator'a без EF.
/// </summary>
public sealed class InMemorySagaRepository : ISagaRepository
{
    private readonly Dictionary<(string, string), SagaInstance> _store = new();
    private readonly List<SagaInstance> _pendingAdd = new();

    public IReadOnlyCollection<SagaInstance> All => _store.Values;

    public ValueTask<SagaInstance?> FindAsync(string sagaType, string correlationKey, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue((sagaType, correlationKey), out var instance);
        return ValueTask.FromResult(instance);
    }

    public ValueTask AddAsync(SagaInstance instance, CancellationToken cancellationToken = default)
    {
        _pendingAdd.Add(instance);
        return ValueTask.CompletedTask;
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var i in _pendingAdd)
            _store[(i.SagaType, i.CorrelationKey)] = i;
        _pendingAdd.Clear();
        return ValueTask.CompletedTask;
    }
}
