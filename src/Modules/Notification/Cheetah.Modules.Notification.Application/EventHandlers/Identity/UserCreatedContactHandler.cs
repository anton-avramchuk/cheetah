using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Notification.Domain.Entities;

namespace Cheetah.Modules.Notification.Application.EventHandlers.Identity;

/// <summary>
/// Создание пользователя в Identity → апсёрт контактной реплики (UserCreatedEvent несёт Email).
/// Смена email требует отдельного события Identity (UserEmailChangedEvent) — см. follow-up в README.
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class UserCreatedContactHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IRepository<RecipientContact, Guid> _contacts;

    public UserCreatedContactHandler(IRepository<RecipientContact, Guid> contacts) => _contacts = contacts;

    public async ValueTask HandleAsync(UserCreatedEvent @event, CancellationToken ct = default)
    {
        var contact = await _contacts.GetByIdAsync(@event.UserId, ct);
        if (contact is null)
        {
            _contacts.Add(RecipientContact.Create(@event.UserId, @event.Email));
        }
        else if (contact.SetEmail(@event.Email))
        {
            _contacts.Update(contact);
        }
        else
        {
            return;
        }

        await _contacts.SaveChangesAsync(ct);
    }
}
