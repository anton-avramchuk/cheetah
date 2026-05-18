using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Inbox;

public static class IdempotentHandlerRegistration
{
    /// <summary>
    /// Регистрирует THandler как Scoped и подменяет IEventHandler&lt;TEvent&gt; на
    /// InboxIdempotentEventHandler&lt;TEvent&gt;, обёрнутый поверх THandler.
    /// </summary>
    public static IServiceCollection AddIdempotentHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IEventHandler<TEvent>>(sp => new InboxIdempotentEventHandler<TEvent>(
            sp.GetRequiredService<THandler>(),
            sp.GetRequiredService<IInboxStore>(),
            sp.GetRequiredService<ILogger<InboxIdempotentEventHandler<TEvent>>>()));
        return services;
    }
}
