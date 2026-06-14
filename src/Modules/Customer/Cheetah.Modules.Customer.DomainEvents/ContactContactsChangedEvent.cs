using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Изменены контактные данные контактного лица (значения — строковые представления VO).</summary>
public record ContactContactsChangedEvent(Guid ContactId, string? Email, string? Phone) : EventBase;
