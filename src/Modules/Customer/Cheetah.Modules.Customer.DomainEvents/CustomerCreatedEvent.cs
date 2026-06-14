using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Клиент создан.</summary>
public record CustomerCreatedEvent(Guid CustomerId, string DisplayName) : EventBase;
