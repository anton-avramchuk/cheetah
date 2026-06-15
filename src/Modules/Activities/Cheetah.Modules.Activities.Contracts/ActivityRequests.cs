using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Contracts;

/// <summary>
/// Базовый запрос на создание активности. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateActivityRequest : CreateActivityRequestBase</c> и добавляет свои поля.
/// </summary>
public abstract record CreateActivityRequestBase
{
    public ActivityType Type { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityPriority Priority { get; init; } = ActivityPriority.Normal;
    public Guid AssigneeId { get; init; }
    public Guid OwnerId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public DateTimeOffset? DueAt { get; init; }
}

/// <summary>Базовый запрос на обновление базовых полей активности.</summary>
public abstract record UpdateActivityRequestBase
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityPriority Priority { get; init; }
    public DateTimeOffset? DueAt { get; init; }
}
