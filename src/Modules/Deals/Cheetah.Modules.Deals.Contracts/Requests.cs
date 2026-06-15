using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Contracts;

// Маршрутные поля помечены [FromRoute]: для GET их подтягивает [AsParameters] по имени,
// для POST — MergeRouteValuesInto в сгенерированном эндпоинте. Тело запроса их не несёт.

// ── Сделки ───────────────────────────────────────────────────────────────────────────────

/// <summary>Создание сделки. Стадия выбирается автоматически (первая Open-стадия воронки).</summary>
public sealed record CreateDealRequest(
    string Title,
    Guid PipelineId,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    Guid OwnerId,
    Guid? ContactId = null,
    DateTimeOffset? ExpectedCloseDate = null) : ICrmRequest;

/// <summary>Сделка по идентификатору.</summary>
public sealed record GetDealByIdRequest(
    [property: FromRoute] Guid DealId) : ICrmRequest;

/// <summary>Список сделок с фильтрами и постраничным выводом.</summary>
public sealed record ListDealsRequest(
    Guid? OwnerId = null,
    Guid? PipelineId = null,
    Guid? StageId = null,
    Guid? CustomerId = null,
    DealStatus? Status = null,
    int Page = 1,
    int Size = 50) : ICrmRequest;

/// <summary>Смена стадии сделки.</summary>
public sealed record ChangeDealStageRequest(
    [property: FromRoute] Guid DealId,
    Guid ToStageId,
    Guid ChangedBy) : ICrmRequest;

/// <summary>Перевод сделки в Won.</summary>
public sealed record WinDealRequest(
    [property: FromRoute] Guid DealId,
    Guid ChangedBy) : ICrmRequest;

/// <summary>Перевод сделки в Lost.</summary>
public sealed record LoseDealRequest(
    [property: FromRoute] Guid DealId,
    string Reason,
    Guid ChangedBy) : ICrmRequest;

/// <summary>Смена ответственного по сделке.</summary>
public sealed record AssignDealOwnerRequest(
    [property: FromRoute] Guid DealId,
    Guid NewOwnerId) : ICrmRequest;

/// <summary>История переходов сделки между стадиями.</summary>
public sealed record GetDealHistoryRequest(
    [property: FromRoute] Guid DealId) : ICrmRequest;

/// <summary>Данные Kanban-доски воронки.</summary>
public sealed record GetDealBoardRequest(
    Guid PipelineId) : ICrmRequest;

// ── Воронки ──────────────────────────────────────────────────────────────────────────────

/// <summary>Создание воронки.</summary>
public sealed record CreatePipelineRequest(
    string Name,
    bool IsDefault = false) : ICrmRequest;

/// <summary>Воронка по идентификатору (со стадиями).</summary>
public sealed record GetPipelineByIdRequest(
    [property: FromRoute] Guid PipelineId) : ICrmRequest;

/// <summary>Список воронок.</summary>
public sealed record ListPipelinesRequest(
    bool ActiveOnly = true) : ICrmRequest;

/// <summary>Добавление стадии в воронку.</summary>
public sealed record AddPipelineStageRequest(
    [property: FromRoute] Guid PipelineId,
    string Name,
    int Order,
    int Probability,
    StageType Type) : ICrmRequest;
