using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Application.Commands;
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

// Конкретные команды хоста для юнит-тестов (базовые команды теперь абстрактные).
public sealed record StubCreateUserCommand(string UserName, string Email, string Password, IReadOnlyList<Guid>? RoleIds = null)
    : CreateUserCommand(UserName, Email, Password, RoleIds);

public sealed record StubUpdateUserCommand(Guid Id, string UserName, string Email, IReadOnlyList<Guid>? RoleIds = null)
    : UpdateUserCommand(Id, UserName, Email, RoleIds);

public sealed record StubCreateRoleCommand(string Name) : CreateRoleCommand(Name);

public sealed record StubUpdateRoleCommand(Guid Id, string Name) : UpdateRoleCommand(Id, Name);

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
