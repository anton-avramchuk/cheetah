using System.Collections.Frozen;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.StateMachine;

/// <summary>
/// Default implementation of <see cref="IStateMachineValidator{TState}"/>.
/// Uses <see cref="FrozenDictionary{TKey,TValue}"/> for O(1) lookup performance.
/// Registered as singleton — built once from <see cref="StateMachineOptions"/>.
/// </summary>
/// <typeparam name="TState">Enum type representing the possible states.</typeparam>
public sealed class StateMachineValidator<TState> : IStateMachineValidator<TState>
    where TState : struct, Enum
{
    private readonly FrozenDictionary<TState, FrozenSet<TState>> _transitions;

    public StateMachineValidator(IOptions<StateMachineOptions> options)
    {
        var builder = options.Value.TryGet<TState>();

        if (builder is null)
            throw new InvalidOperationException(
                $"No state machine configuration found for '{typeof(TState).Name}'. " +
                $"Register it via services.Configure<StateMachineOptions>(o => o.For<{typeof(TState).Name}>()...).");

        _transitions = builder.Build()
            .ToFrozenDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToFrozenSet());
    }

    /// <inheritdoc />
    public bool CanTransition(TState from, TState to)
    {
        return _transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <inheritdoc />
    public void ValidateTransition(TState from, TState to)
    {
        if (!CanTransition(from, to))
            throw InvalidStateTransitionException.For(from, to);
    }

    /// <inheritdoc />
    public IReadOnlyList<TState> GetAllowedTransitions(TState from)
    {
        return _transitions.TryGetValue(from, out var allowed)
            ? [..allowed]
            : [];
    }
}
