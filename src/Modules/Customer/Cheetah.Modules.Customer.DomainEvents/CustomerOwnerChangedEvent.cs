using Cheetah.Core.Events;

namespace Cheetah.Modules.Customer.DomainEvents;

/// <summary>
/// Изменён закреплённый ответственный (наш сотрудник из Identity) за клиентом.
/// <paramref name="OwnerId"/> = null — ответственный снят.
/// </summary>
public record CustomerOwnerChangedEvent(Guid CustomerId, Guid? OwnerId) : EventBase;
