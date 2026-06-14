using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Customer.DomainEvents;
using Cheetah.Modules.Customer.Shared;

namespace Cheetah.Modules.Customer.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат клиента. Шаблонный модуль не инстанцирует его сам —
/// наследник объявляет конкретный <c>sealed class Customer : CustomerBase</c> со своей
/// фабрикой <c>Create(...)</c> и доп. полями. Контакты хранятся как value objects
/// (<see cref="Email"/>/<see cref="Phone"/>); на границах модуля используются строки.
/// </summary>
public abstract class CustomerBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public string DisplayName { get; private set; } = null!;
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public CustomerStatus Status { get; private set; }

    /// <summary>Закреплённый ответственный — пользователь Identity (ссылка по Id, без FK через границу модуля).</summary>
    public Guid? OwnerId { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected CustomerBase() { } // EF

    /// <summary>
    /// Заводит инварианты нового клиента и доменное событие создания. Вызывается
    /// фабрикой наследника. Контакты передаются строками и валидируются через VO.
    /// </summary>
    protected void InitializeCore(Guid id, string displayName, string? email, string? phone, Guid? ownerId = null)
    {
        Id = id;
        SetDisplayName(displayName);
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        OwnerId = ownerId;
        Status = CustomerStatus.Active;
        AddDomainEvent(new CustomerCreatedEvent(Id, DisplayName));
    }

    public void Rename(string displayName)
    {
        SetDisplayName(displayName);
        AddDomainEvent(new CustomerRenamedEvent(Id, DisplayName));
    }

    public void ChangeContacts(string? email, string? phone)
    {
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        AddDomainEvent(new CustomerContactsChangedEvent(Id, Email?.Value, Phone?.Value));
    }

    /// <summary>Назначить/снять ответственного. Событие — только при фактическом изменении.</summary>
    public void AssignOwner(Guid? ownerId)
    {
        if (OwnerId == ownerId)
            return;

        OwnerId = ownerId;
        AddDomainEvent(new CustomerOwnerChangedEvent(Id, OwnerId));
    }

    public void Activate() => Status = CustomerStatus.Active;

    public void Deactivate() => Status = CustomerStatus.Inactive;

    public void Archive()
    {
        if (Status == CustomerStatus.Archived)
            return;

        Status = CustomerStatus.Archived;
        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CustomerArchivedEvent(Id));
    }

    private void SetDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
    }

    private static Email? ParseEmail(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : Cheetah.Core.Domain.ValueObjects.Email.Create(value);

    private static Phone? ParsePhone(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : Cheetah.Core.Domain.ValueObjects.Phone.Create(value);
}
