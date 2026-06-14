using Cheetah.Core.Events;

namespace Cheetah.Modules.Identity.DomainEvents;

/// <summary>Пользователь создан.</summary>
public record UserCreatedEvent(Guid UserId, string UserName, string Email) : EventBase;
