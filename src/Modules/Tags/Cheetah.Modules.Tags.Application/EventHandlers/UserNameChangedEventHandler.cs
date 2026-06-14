using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Tags.Domain.Abstractions;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Application.EventHandlers;

/// <summary>Смена username в Identity → обновление локальной реплики.</summary>
[Export(LifetimeType.Scoped)]
public class UserNameChangedEventHandler : IEventHandler<UserNameChangedEvent>
{
    private readonly IRepository<User, Guid> _users;

    public UserNameChangedEventHandler(IRepository<User, Guid> users) => _users = users;

    public async ValueTask HandleAsync(UserNameChangedEvent @event, CancellationToken ct = default)
    {
        var entry = new UserDirectoryEntry(@event.UserId, @event.NewUserName);
        var user = await _users.GetByIdAsync(@event.UserId, ct);

        if (user is null)
            _users.Add(User.Create(entry));
        else if (user.Apply(entry))
            _users.Update(user);
        else
            return; // содержимое не изменилось

        await _users.SaveChangesAsync(ct);
    }
}
