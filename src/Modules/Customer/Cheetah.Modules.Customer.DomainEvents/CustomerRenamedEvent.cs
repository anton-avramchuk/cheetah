using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Изменено отображаемое имя клиента.</summary>
public record CustomerRenamedEvent(Guid CustomerId, string DisplayName) : EventBase;
