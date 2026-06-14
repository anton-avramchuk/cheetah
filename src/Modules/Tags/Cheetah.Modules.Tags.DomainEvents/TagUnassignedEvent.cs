using Cheetah.Core.Events;

namespace Cheetah.Modules.Tags.DomainEvents;

/// <summary>Тэг снят с сущности другого сервиса.</summary>
public record TagUnassignedEvent(Guid TagId, string EntityType, Guid EntityId) : EventBase;
