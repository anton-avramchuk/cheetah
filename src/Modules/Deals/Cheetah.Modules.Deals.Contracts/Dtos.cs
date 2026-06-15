using Cheetah.Contracts.Responses;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Contracts;

/// <summary>Сделка (полная карточка).</summary>
public sealed record DealDto(
    Guid Id,
    string Title,
    Guid PipelineId,
    Guid StageId,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    Guid? ContactId,
    Guid OwnerId,
    DateTimeOffset? ExpectedCloseDate,
    DealStatus Status,
    string? LostReason,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt) : ICrmResponse;

/// <summary>Сделка в списке (облегчённая проекция).</summary>
public sealed record DealListItemDto(
    Guid Id,
    string Title,
    Guid PipelineId,
    Guid StageId,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    Guid OwnerId,
    DealStatus Status,
    DateTimeOffset? ExpectedCloseDate) : ICrmResponse;

/// <summary>Запись истории перехода сделки между стадиями.</summary>
public sealed record DealHistoryDto(
    Guid Id,
    Guid FromStageId,
    Guid ToStageId,
    Guid ChangedBy,
    DateTimeOffset? ChangedAt) : ICrmResponse;

/// <summary>Воронка с её стадиями.</summary>
public sealed record PipelineDto(
    Guid Id,
    string Name,
    bool IsDefault,
    bool IsActive,
    IReadOnlyList<PipelineStageDto> Stages) : ICrmResponse;

/// <summary>Стадия воронки.</summary>
public sealed record PipelineStageDto(
    Guid Id,
    string Name,
    int Order,
    int Probability,
    StageType Type) : ICrmResponse;

/// <summary>Колонка Kanban-доски: стадия + агрегаты по открытым сделкам.</summary>
public sealed record BoardColumnDto(
    Guid StageId,
    int Count,
    decimal Sum) : ICrmResponse;

/// <summary>Данные Kanban-доски воронки.</summary>
public sealed record BoardDto(
    Guid PipelineId,
    IReadOnlyList<BoardColumnDto> Columns) : ICrmResponse;
