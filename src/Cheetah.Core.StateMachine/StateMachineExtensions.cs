using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.StateMachine;

/// <summary>
/// Extension methods for registering state machine services.
/// </summary>
public static class StateMachineExtensions
{
    /// <summary>
    /// Registers core state machine services (open-generic validator).
    /// Called automatically by <see cref="CrmStateMachineModule"/>.
    /// </summary>
    public static IServiceCollection AddStateMachine(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(IStateMachineValidator<>), typeof(StateMachineValidator<>));
        return services;
    }

    /// <summary>
    /// Configures a state machine for a specific state enum and registers services.
    /// Shorthand for <c>Configure&lt;StateMachineOptions&gt;</c> + <c>AddStateMachine()</c>.
    /// <code>
    /// services.AddStateMachine&lt;OrderState&gt;(sm => sm
    ///     .From(OrderState.New).To(OrderState.Confirmed, OrderState.Cancelled)
    ///     .From(OrderState.Confirmed).To(OrderState.Shipped));
    /// </code>
    /// </summary>
    public static IServiceCollection AddStateMachine<TState>(
        this IServiceCollection services,
        Action<StateMachineBuilder<TState>> configure)
        where TState : struct, Enum
    {
        services.AddStateMachine();
        services.Configure<StateMachineOptions>(options => configure(options.For<TState>()));
        return services;
    }
}
