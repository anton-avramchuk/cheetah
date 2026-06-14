using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Контактное лицо удалено (мягкое удаление).</summary>
public record ContactRemovedEvent(Guid ContactId) : EventBase;
