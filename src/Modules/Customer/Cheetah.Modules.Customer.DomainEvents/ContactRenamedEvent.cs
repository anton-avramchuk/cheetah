using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>Изменено имя контактного лица.</summary>
public record ContactRenamedEvent(Guid ContactId, string FullName) : EventBase;
