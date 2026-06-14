using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Tests;

public sealed class StubRole : IdentityRole
{
    public StubRole() : base(Guid.NewGuid(), "stub") { }
    public StubRole(Guid id, string name) : base(id, name) { }
}

public sealed class StubUser : IdentityUser<StubRole>
{
    public StubUser(string userName, string email) : base(Guid.NewGuid(), userName, email) { }
}

/// <summary>No-op шина событий для юнит-тестов хендлеров.</summary>
public sealed class NullEventBus : IEventBus
{
    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent => ValueTask.CompletedTask;

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent => ValueTask.CompletedTask;

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent> { }
}
