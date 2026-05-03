namespace Cheetah.Core.StateMachine;

/// <summary>
/// Thrown when an invalid state transition is attempted.
/// </summary>
public class InvalidStateTransitionException : InvalidOperationException
{
    public string FromState { get; }
    public string ToState { get; }
    public Type StateType { get; }

    public InvalidStateTransitionException(Type stateType, object from, object to)
        : base($"Transition from '{from}' to '{to}' is not allowed for state machine '{stateType.Name}'.")
    {
        StateType = stateType;
        FromState = from.ToString()!;
        ToState = to.ToString()!;
    }

    public InvalidStateTransitionException(Type stateType, object from, object to, string message)
        : base(message)
    {
        StateType = stateType;
        FromState = from.ToString()!;
        ToState = to.ToString()!;
    }

    public static InvalidStateTransitionException For<TState>(TState from, TState to)
        where TState : struct, Enum
        => new(typeof(TState), from, to);

    public static InvalidStateTransitionException For<TState>(TState from, TState to, string message)
        where TState : struct, Enum
        => new(typeof(TState), from, to, message);
}
