using Cheetah.Core.Events;

namespace Cheetah.Modules.Deals.DomainEvents;

// Чистые контракты интеграционных событий модуля сделок. Публикуются через шину
// (транзакционный Outbox) в той же транзакции, что и SaveChangesAsync. DomainEvents не зависит
// от Shared/Domain — суммы/валюты передаются примитивами.

/// <summary>Сделка создана.</summary>
public record DealCreatedIntegrationEvent(
    Guid DealId,
    Guid CustomerId,
    Guid PipelineId,
    Guid StageId,
    decimal Amount,
    string Currency,
    Guid OwnerId) : EventBase;

/// <summary>Сделка перемещена на другую стадию (в пределах открытого статуса).</summary>
public record DealStageChangedIntegrationEvent(
    Guid DealId,
    Guid FromStageId,
    Guid ToStageId,
    Guid ChangedBy) : EventBase;

/// <summary>Сделка выиграна (терминальный статус).</summary>
public record DealWonIntegrationEvent(
    Guid DealId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTimeOffset ClosedAt) : EventBase;

/// <summary>Сделка проиграна (терминальный статус).</summary>
public record DealLostIntegrationEvent(
    Guid DealId,
    Guid CustomerId,
    string Reason,
    DateTimeOffset ClosedAt) : EventBase;

/// <summary>Сменился ответственный по сделке.</summary>
public record DealOwnerChangedIntegrationEvent(
    Guid DealId,
    Guid OldOwnerId,
    Guid NewOwnerId) : EventBase;
