using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат лида. Наследник объявляет <c>sealed class Lead : LeadBase</c> со своими
/// полями (точка расширяемости); скоринг расширяется через <see cref="CalculateScore"/>.
/// <para>
/// Статус и источник — данные (справочники <see cref="LeadStatus"/>/<see cref="LeadSource"/>), лид
/// ссылается на них по <see cref="StatusId"/>/<see cref="SourceId"/>. Жизненный цикл валидируется
/// доменно по стабильным well-known идентификаторам (<see cref="LeadWellKnownIds"/>), без enum-автомата.
/// </para>
/// </summary>
public abstract class LeadBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public string FullName { get; private set; } = null!;
    public string? Company { get; private set; }
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public Guid SourceId { get; private set; }
    public Guid StatusId { get; private set; }
    public int Score { get; private set; }
    public Guid? OwnerId { get; private set; }
    public Guid? ConvertedCustomerId { get; private set; }
    public Guid? ConvertedDealId { get; private set; }
    public string? DisqualifyReason { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb). Полноценно — модуль Custom Fields.</summary>
    public string? Attributes { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected LeadBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты нового лида и доменное событие создания. Вызывается фабрикой наследника.
    /// Статус ставится в New (well-known id). Контакты передаются строками и валидируются через VO.
    /// </summary>
    protected void InitializeCore(Guid id, string fullName, Guid sourceId,
        string? email, string? phone, string? company, Guid? ownerId)
    {
        if (sourceId == Guid.Empty)
            throw new ArgumentException("SourceId is required.", nameof(sourceId));

        Id = id;
        SetFullName(fullName);
        SourceId = sourceId;
        Company = company;
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        OwnerId = ownerId;
        StatusId = LeadWellKnownIds.StatusNew;
        Score = CalculateScore();
        AddDomainEvent(new LeadCreatedIntegrationEvent(Id, SourceId));
    }

    public virtual void StartWorking()
    {
        if (StatusId == LeadWellKnownIds.StatusNew)
            StatusId = LeadWellKnownIds.StatusWorking;
    }

    public virtual void Qualify()
    {
        if (StatusId != LeadWellKnownIds.StatusNew && StatusId != LeadWellKnownIds.StatusWorking)
            throw new InvalidOperationException("Only a new/working lead can be qualified.");

        StatusId = LeadWellKnownIds.StatusQualified;
        AddDomainEvent(new LeadQualifiedIntegrationEvent(Id));
    }

    public virtual void Disqualify(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A disqualify reason is required.", nameof(reason));
        if (StatusId == LeadWellKnownIds.StatusConverted)
            throw new InvalidOperationException("A converted lead cannot be disqualified.");

        StatusId = LeadWellKnownIds.StatusDisqualified;
        DisqualifyReason = reason.Trim();
        AddDomainEvent(new LeadDisqualifiedIntegrationEvent(Id, DisqualifyReason));
    }

    /// <summary>Фиксация результата конвертации после успеха оркестратора (см. Application).</summary>
    public virtual void MarkConverted(Guid customerId, Guid? dealId)
    {
        if (StatusId == LeadWellKnownIds.StatusConverted)
            return;

        StatusId = LeadWellKnownIds.StatusConverted;
        ConvertedCustomerId = customerId;
        ConvertedDealId = dealId;
        AddDomainEvent(new LeadConvertedIntegrationEvent(Id, customerId, dealId));
    }

    public virtual void AssignOwner(Guid? ownerId) => OwnerId = ownerId;

    public virtual void Update(string fullName, string? company, string? email, string? phone)
    {
        SetFullName(fullName);
        Company = company;
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        Score = CalculateScore();
    }

    public void SetAttributes(string? attributes) => Attributes = attributes;

    /// <summary>Переопределяемое правило скоринга — точка расширения.</summary>
    protected virtual int CalculateScore()
    {
        var s = 0;
        if (Email is not null) s += 30;
        if (Phone is not null) s += 30;
        if (!string.IsNullOrWhiteSpace(Company)) s += 20;
        if (SourceId == LeadWellKnownIds.SourceReferral) s += 20;
        else if (SourceId == LeadWellKnownIds.SourceWeb) s += 10;
        return Math.Min(s, 100);
    }

    private void SetFullName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        FullName = value.Trim();
    }

    private static Email? ParseEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Email.Create(value);

    private static Phone? ParsePhone(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Cheetah.Core.Domain.ValueObjects.Phone.Create(value);
}
