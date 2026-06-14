using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Notification.Domain.Entities;

namespace Cheetah.Modules.Notification.Application.EventHandlers.Identity;

/// <summary>Удаление пользователя в Identity → удаление контактной реплики.</summary>
[Export(LifetimeType.Scoped)]
public sealed class UserDeletedContactHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IRepository<RecipientContact, Guid> _contacts;

    public UserDeletedContactHandler(IRepository<RecipientContact, Guid> contacts) => _contacts = contacts;

    public async ValueTask HandleAsync(UserDeletedEvent @event, CancellationToken ct = default)
    {
        var contact = await _contacts.GetByIdAsync(@event.UserId, ct);
        if (contact is null)
            return;

        _contacts.Delete(contact);
        await _contacts.SaveChangesAsync(ct);
    }
}
