namespace Cheetah.Core.StateMachine;

/// <summary>
/// Fluent builder for defining allowed state transitions.
/// <code>
/// builder.From(State.Draft).To(State.Active, State.Cancelled);
/// builder.From(State.Active).To(State.Completed, State.Cancelled);
/// </code>
/// </summary>
/// <typeparam name="TState">Enum type representing the possible states.</typeparam>
public sealed class StateMachineBuilder<TState> where TState : struct, Enum
{
    private readonly Dictionary<TState, HashSet<TState>> _transitions = new();

    /// <summary>
    /// Starts defining transitions from a given state.
    /// </summary>
    public TransitionSource From(TState from)
    {
        if (!_transitions.ContainsKey(from))
            _transitions[from] = new HashSet<TState>();

        return new TransitionSource(this, from);
    }

    internal void AddTransition(TState from, TState to)
    {
        if (!_transitions.TryGetValue(from, out var targets))
        {
            targets = new HashSet<TState>();
            _transitions[from] = targets;
        }

        targets.Add(to);
    }

    internal IReadOnlyDictionary<TState, HashSet<TState>> Build() => _transitions;

    /// <summary>
    /// Fluent source-side of a transition definition.
    /// </summary>
    public readonly struct TransitionSource
    {
        private readonly StateMachineBuilder<TState> _builder;
        private readonly TState _from;

        internal TransitionSource(StateMachineBuilder<TState> builder, TState from)
        {
            _builder = builder;
            _from = from;
        }

        /// <summary>
        /// Defines one or more allowed target states from the source state.
        /// </summary>
        public StateMachineBuilder<TState> To(params TState[] targets)
        {
            foreach (var target in targets)
                _builder.AddTransition(_from, target);

            return _builder;
        }
    }
}
