using Cheetah.Core.Events;

namespace Cheetah.Modules.Tags.DomainEvents;

/// <summary>Тэг назначен сущности другого сервиса. Потребитель может обновить свою денормализованную проекцию.</summary>
public record TagAssignedEvent(Guid TagId, string EntityType, Guid EntityId) : EventBase;
