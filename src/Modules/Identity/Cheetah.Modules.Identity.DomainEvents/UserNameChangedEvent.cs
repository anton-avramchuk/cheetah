using Cheetah.Core.Events;

namespace Cheetah.Modules.Identity.DomainEvents;

/// <summary>Имя пользователя (username) изменено.</summary>
public record UserNameChangedEvent(Guid UserId, string OldUserName, string NewUserName) : EventBase;
