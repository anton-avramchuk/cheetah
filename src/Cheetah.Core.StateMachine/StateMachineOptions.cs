namespace Cheetah.Core.StateMachine;

/// <summary>
/// Central options object that holds state machine configurations for multiple state types.
/// Configured via <c>services.Configure&lt;StateMachineOptions&gt;(options => { ... });</c>
/// </summary>
public sealed class StateMachineOptions
{
    internal Dictionary<Type, object> Builders { get; } = new();

    /// <summary>
    /// Returns (or creates) a <see cref="StateMachineBuilder{TState}"/> for the given state enum.
    /// Multiple calls for the same TState return the same builder instance.
    /// <code>
    /// options.For&lt;ResearchState&gt;()
    ///     .From(ResearchState.InWork).To(ResearchState.Complete, ResearchState.Cancelled)
    ///     .From(ResearchState.Complete).To(ResearchState.Edit, ResearchState.Sent);
    /// </code>
    /// </summary>
    public StateMachineBuilder<TState> For<TState>() where TState : struct, Enum
    {
        var key = typeof(TState);
        if (Builders.TryGetValue(key, out var existing))
            return (StateMachineBuilder<TState>)existing;

        var builder = new StateMachineBuilder<TState>();
        Builders[key] = builder;
        return builder;
    }

    internal StateMachineBuilder<TState>? TryGet<TState>() where TState : struct, Enum
    {
        return Builders.TryGetValue(typeof(TState), out var builder)
            ? (StateMachineBuilder<TState>)builder
            : null;
    }
}
