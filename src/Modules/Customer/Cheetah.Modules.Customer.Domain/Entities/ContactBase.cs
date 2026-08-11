using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Customer.DomainEvents;

namespace Cheetah.Modules.Customer.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат контактного лица клиента. Отдельный агрегат (не child-entity
/// клиента) со ссылкой <see cref="CustomerId"/> — клиент остаётся лёгким, контакты листаются
/// и изменяются независимо. Generic над типом должности <typeparamref name="TPosition"/>
/// (наследник <see cref="PositionBase"/>): контакт ссылается на справочную должность по
/// <see cref="PositionId"/> + строго типизированная навигация <see cref="Position"/>. Шаблонный
/// модуль не инстанцирует его сам: наследник объявляет конкретный
/// <c>sealed class Contact : ContactBase&lt;Position&gt;</c> со своей фабрикой и доп. полями.
/// </summary>
public abstract class ContactBase<TPosition> : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
    where TPosition : PositionBase
{
    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; } = null!;

    /// <summary>Ссылка на должность-справочник (FK). Меняется через <see cref="ChangePosition"/>.</summary>
    public Guid? PositionId { get; private set; }

    /// <summary>Навигация на должность (заполняется EF при загрузке); домен меняет только <see cref="PositionId"/>.</summary>
    public TPosition? Position { get; private set; }

    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }

    /// <summary>
    /// Мессенджеры — отдельными полями, по одному на вид. Списком их держать заманчиво, но тогда
    /// интерфейс не может обещать кнопку: у «канала» неизвестного вида некуда вести. Типизированное
    /// поле, наоборот, гарантирует, что значение годно для ссылки.
    /// </summary>
    public Telegram? Telegram { get; private set; }
    public WhatsApp? WhatsApp { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected ContactBase() { } // EF

    /// <summary>
    /// Заводит инварианты нового контактного лица и событие добавления. Вызывается фабрикой
    /// наследника. Контакты передаются строками и валидируются через VO.
    /// </summary>
    protected void InitializeCore(
        Guid id,
        Guid customerId,
        string fullName,
        Guid? positionId,
        string? email,
        string? phone,
        string? telegram = null,
        string? whatsApp = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        Id = id;
        CustomerId = customerId;
        SetFullName(fullName);
        PositionId = positionId;
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        Telegram = ParseTelegram(telegram);
        WhatsApp = ParseWhatsApp(whatsApp);
        AddDomainEvent(new ContactAddedEvent(Id, CustomerId, FullName));
    }

    public void Rename(string fullName)
    {
        SetFullName(fullName);
        AddDomainEvent(new ContactRenamedEvent(Id, FullName));
    }

    public void ChangePosition(Guid? positionId) => PositionId = positionId;

    /// <summary>
    /// Способы связи целиком: не присланное значение очищается, а не остаётся прежним — так это
    /// правит форма, где все поля видны сразу.
    ///
    /// Мессенджеры — необязательные параметры: наследники, заведённые до их появления, продолжают
    /// собираться и просто их не заполняют.
    /// </summary>
    public void ChangeContacts(string? email, string? phone, string? telegram = null, string? whatsApp = null)
    {
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        Telegram = ParseTelegram(telegram);
        WhatsApp = ParseWhatsApp(whatsApp);

        // Состав события не расширяется: он контракт с потребителями шины, а мессенджеры нужны
        // карточке, а не им. Расширить его — значит переиздать контракт ради поля, которое никто не
        // читает.
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

    private static Email? ParseEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Email.Create(value);

    private static Phone? ParsePhone(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Phone.Create(value);

    private static Telegram? ParseTelegram(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Telegram.Create(value);

    private static WhatsApp? ParseWhatsApp(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.WhatsApp.Create(value);
}
