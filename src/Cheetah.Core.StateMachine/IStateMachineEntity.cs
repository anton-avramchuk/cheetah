namespace Cheetah.Core.StateMachine;

/// <summary>
/// Marker interface for entities that participate in a state machine.
/// </summary>
/// <typeparam name="TState">Enum type representing the possible states.</typeparam>
public interface IStateMachineEntity<TState> where TState : struct, Enum
{
    TState State { get; }
}
