namespace Cheetah.Core.StateMachine;

/// <summary>
/// Validates state transitions for a given state enum type.
/// Resolved from DI as a singleton — one validator per TState.
/// </summary>
/// <typeparam name="TState">Enum type representing the possible states.</typeparam>
public interface IStateMachineValidator<TState> where TState : struct, Enum
{
    /// <summary>
    /// Returns true if transitioning from <paramref name="from"/> to <paramref name="to"/> is allowed.
    /// </summary>
    bool CanTransition(TState from, TState to);

    /// <summary>
    /// Validates the transition. Throws <see cref="InvalidStateTransitionException"/> if not allowed.
    /// </summary>
    void ValidateTransition(TState from, TState to);

    /// <summary>
    /// Returns all states that can be reached from <paramref name="from"/>.
    /// </summary>
    IReadOnlyList<TState> GetAllowedTransitions(TState from);
}
