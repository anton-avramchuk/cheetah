using Cheetah.Contracts.Responses;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Contracts;

/// <summary>
/// Базовый ViewModel активности (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record ActivityDto : ActivityDtoBase</c> и при необходимости добавляет свои поля
/// (например, <c>CallOutcome</c>, <c>MeetingUrl</c>). Это и есть точка расширяемости ViewModel.
/// </summary>
public abstract record ActivityDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public ActivityType Type { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityStatus Status { get; init; }
    public ActivityPriority Priority { get; init; }
    public Guid AssigneeId { get; init; }
    public Guid OwnerId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public DateTimeOffset? DueAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? Result { get; init; }
    public Guid? CalendarEventId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
