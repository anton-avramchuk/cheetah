using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Customer.DomainEvents;

namespace Cheetah.Modules.Customer.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат контактного лица клиента. Отдельный агрегат (не child-entity
/// клиента) со ссылкой <see cref="CustomerId"/> — клиент остаётся лёгким, контакты листаются
/// и изменяются независимо. Шаблонный модуль не инстанцирует его сам: наследник объявляет
/// конкретный <c>sealed class Contact : ContactBase</c> со своей фабрикой и доп. полями.
/// </summary>
public abstract class ContactBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; } = null!;
    public string? Position { get; private set; }
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected ContactBase() { } // EF

    /// <summary>
    /// Заводит инварианты нового контактного лица и событие добавления. Вызывается фабрикой
    /// наследника. Контакты передаются строками и валидируются через VO.
    /// </summary>
    protected void InitializeCore(Guid id, Guid customerId, string fullName, string? position, string? email, string? phone)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        Id = id;
        CustomerId = customerId;
        SetFullName(fullName);
        Position = NormalizeOptional(position);
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        AddDomainEvent(new ContactAddedEvent(Id, CustomerId, FullName));
    }

    public void Rename(string fullName)
    {
        SetFullName(fullName);
        AddDomainEvent(new ContactRenamedEvent(Id, FullName));
    }

    public void ChangePosition(string? position) => Position = NormalizeOptional(position);

    public void ChangeContacts(string? email, string? phone)
    {
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        AddDomainEvent(new ContactContactsChangedEvent(Id, Email?.Value, Phone?.Value));
    }

    public void Remove()
    {
        if (RemovedAt is not null)
            return;

        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ContactRemovedEvent(Id));
    }

    private void SetFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Email? ParseEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Email.Create(value);

    private static Phone? ParsePhone(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Phone.Create(value);
}
