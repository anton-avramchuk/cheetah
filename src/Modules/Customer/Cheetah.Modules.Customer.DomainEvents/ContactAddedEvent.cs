using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Контактное лицо добавлено к клиенту.</summary>
public record ContactAddedEvent(Guid ContactId, Guid CustomerId, string FullName) : EventBase;
