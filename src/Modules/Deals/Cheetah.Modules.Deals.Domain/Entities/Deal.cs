using Cheetah.Core.Domain;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Deals.DomainEvents;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Domain.Entities;

/// <summary>
/// Сделка — агрегат. Статус (<see cref="DealStatus"/>) участвует в конечном автомате через
/// <see cref="IStateMachineEntity{TState}"/>: <c>Open → Won|Lost</c> и reopen <c>Won|Lost → Open</c>.
/// Перемещение по стадиям внутри Open валидируется доменно (стадии — данные, не enum).
/// </summary>
public sealed class Deal : AggregateRoot<Guid>, IStateMachineEntity<DealStatus>,
    ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<DealStageHistory> _history = new();

    public string Title { get; private set; } = null!;
    public Guid PipelineId { get; private set; }
    public Guid StageId { get; private set; }
    public Money Value { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTimeOffset? ExpectedCloseDate { get; private set; }
    public DealStatus Status { get; private set; }
    public string? LostReason { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public IReadOnlyList<DealStageHistory> History => _history;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public DealStatus State => Status;

    private Deal() { } // EF

    public static Deal Create(string title, Pipeline pipeline, Money value,
        Guid customerId, Guid ownerId, Guid? contactId = null, DateTimeOffset? expectedCloseDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(value);

        var firstStage = pipeline.FirstStage();
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            PipelineId = pipeline.Id,
            StageId = firstStage.Id,
            Value = value,
            CustomerId = customerId,
            ContactId = contactId,
            OwnerId = ownerId,
            ExpectedCloseDate = expectedCloseDate,
            Status = DealStatus.Open
        };

        deal.AddDomainEvent(new DealCreatedIntegrationEvent(
            deal.Id, customerId, pipeline.Id, firstStage.Id, value.Amount, value.Currency, ownerId));
        return deal;
    }

    /// <summary>
    /// Перемещение по стадиям в пределах открытого статуса. Попадание на стадию типа Won
    /// терминализует сделку; для Lost требуется явный <see cref="Lose"/> (нужна причина).
    /// </summary>
    public void MoveToStage(PipelineStage target, Guid changedBy)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Closed deal cannot change stage.");
        if (target.PipelineId != PipelineId)
            throw new InvalidOperationException("Stage belongs to another pipeline.");
        if (target.Id == StageId)
            return;

        var from = StageId;
        _history.Add(DealStageHistory.Create(Id, from, target.Id, changedBy));
        StageId = target.Id;

        if (target.Type == StageType.Won)
        {
            WinInternal();
        }
        else if (target.Type == StageType.Lost)
        {
            throw new InvalidOperationException("Use Lose(reason) to move a deal to a Lost stage.");
        }
        else
        {
            AddDomainEvent(new DealStageChangedIntegrationEvent(Id, from, target.Id, changedBy));
        }
    }

    public void Win(Guid changedBy)
    {
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Only an open deal can be won.");
        WinInternal();
    }

    private void WinInternal()
    {
        // Доменный инвариант: нельзя выиграть без положительной суммы.
        if (Value.Amount <= 0)
            throw new InvalidOperationException("A won deal requires a positive amount.");

        Status = DealStatus.Won;
        ClosedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new DealWonIntegrationEvent(Id, CustomerId, Value.Amount, Value.Currency, ClosedAt.Value));
    }

    public void Lose(string reason, Guid changedBy)
    {
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Only an open deal can be lost.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A lost reason is required.", nameof(reason));

        Status = DealStatus.Lost;
        LostReason = reason.Trim();
        ClosedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new DealLostIntegrationEvent(Id, CustomerId, LostReason, ClosedAt.Value));
    }

    /// <summary>Возврат закрытой сделки в работу на указанную стадию.</summary>
    public void Reopen(Guid stageId)
    {
        if (Status == DealStatus.Open)
            return;

        Status = DealStatus.Open;
        ClosedAt = null;
        LostReason = null;
        StageId = stageId;
    }

    public void AssignOwner(Guid newOwnerId)
    {
        if (newOwnerId == OwnerId)
            return;

        var old = OwnerId;
        OwnerId = newOwnerId;
        AddDomainEvent(new DealOwnerChangedIntegrationEvent(Id, old, newOwnerId));
    }

    public void ChangeValue(Money value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    public void Rename(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
    }

    public void ChangeExpectedCloseDate(DateTimeOffset? date) => ExpectedCloseDate = date;
}
