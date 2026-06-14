using Cheetah.Core.Events;

namespace Cheetah.Modules.Tags.DomainEvents;

/// <summary>Тэг создан в словаре.</summary>
public record TagCreatedEvent(Guid TagId, string Name) : EventBase;
