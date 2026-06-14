using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Application.EventHandlers;

/// <summary>Удаление пользователя в Identity → удаление локальной реплики.</summary>
[Export(LifetimeType.Scoped)]
public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IRepository<User, Guid> _users;

    public UserDeletedEventHandler(IRepository<User, Guid> users) => _users = users;

    public async ValueTask HandleAsync(UserDeletedEvent @event, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(@event.UserId, ct);
        if (user is null)
            return;

        _users.Delete(user);
        await _users.SaveChangesAsync(ct);
    }
}
