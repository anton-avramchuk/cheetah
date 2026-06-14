using Cheetah.Core.Events;

namespace Cheetah.Modules.Identity.DomainEvents;

/// <summary>Пользователь удалён.</summary>
public record UserDeletedEvent(Guid UserId, string UserName) : EventBase;
