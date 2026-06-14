using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Изменены контактные данные клиента (значения — строковые представления VO).</summary>
public record CustomerContactsChangedEvent(Guid CustomerId, string? Email, string? Phone) : EventBase;
