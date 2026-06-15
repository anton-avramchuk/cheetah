using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат лида. Шаблонный модуль не инстанцирует его сам — наследник объявляет
/// конкретный <c>sealed class Lead : LeadBase</c> со своей фабрикой (через <see cref="InitializeCore"/>)
/// и доп. полями. Это точка расширяемости сущности; скоринг расширяется через переопределение
/// <see cref="CalculateScore"/>.
/// <para>
/// Жизненный цикл (<see cref="LeadStatus"/>) участвует в конечном автомате через
/// <see cref="IStateMachineEntity{TState}"/>. Контакты — value objects (<see cref="Email"/>/<see cref="Phone"/>);
/// на границе API — строки.
/// </para>
/// </summary>
public abstract class LeadBase : AggregateRoot<Guid>,
    IStateMachineEntity<LeadStatus>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public string FullName { get; private set; } = null!;
    public string? Company { get; private set; }
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
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

    /// <inheritdoc />
    public LeadStatus State => Status;

    protected LeadBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты нового лида и доменное событие создания. Вызывается фабрикой наследника.
    /// Контакты передаются строками и валидируются через VO.
    /// </summary>
    protected void InitializeCore(Guid id, string fullName, LeadSource source,
        string? email, string? phone, string? company, Guid? ownerId)
    {
        Id = id;
        SetFullName(fullName);
        Source = source;
        Company = company;
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        OwnerId = ownerId;
        Status = LeadStatus.New;
        Score = CalculateScore();
        AddDomainEvent(new LeadCreatedIntegrationEvent(Id, source.ToString()));
    }

    public virtual void StartWorking()
    {
        if (Status == LeadStatus.New)
            Status = LeadStatus.Working;
    }

    public virtual void Qualify()
    {
        if (Status is not (LeadStatus.New or LeadStatus.Working))
            throw new InvalidOperationException("Only a new/working lead can be qualified.");

        Status = LeadStatus.Qualified;
        AddDomainEvent(new LeadQualifiedIntegrationEvent(Id));
    }

    public virtual void Disqualify(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A disqualify reason is required.", nameof(reason));
        if (Status == LeadStatus.Converted)
            throw new InvalidOperationException("A converted lead cannot be disqualified.");

        Status = LeadStatus.Disqualified;
        DisqualifyReason = reason.Trim();
        AddDomainEvent(new LeadDisqualifiedIntegrationEvent(Id, DisqualifyReason));
    }

    /// <summary>Фиксация результата конвертации после успеха оркестратора (см. Application).</summary>
    public virtual void MarkConverted(Guid customerId, Guid? dealId)
    {
        if (Status == LeadStatus.Converted)
            return;

        Status = LeadStatus.Converted;
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
        s += Source switch { LeadSource.Referral => 20, LeadSource.Web => 10, _ => 0 };
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
