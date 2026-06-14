using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Клиент архивирован (мягкое удаление).</summary>
public record CustomerArchivedEvent(Guid CustomerId) : EventBase;
