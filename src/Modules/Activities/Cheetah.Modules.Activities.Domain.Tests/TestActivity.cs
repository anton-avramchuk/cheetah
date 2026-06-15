using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="ActivityBase"/> для проверки базового поведения. Доп. поле
/// <see cref="CallOutcome"/> демонстрирует расширяемость сущности.
/// </summary>
public sealed class TestActivity : ActivityBase
{
    public string? CallOutcome { get; private set; }

    private TestActivity() { }

    public static TestActivity Create(
        ActivityType type, string title, Guid assigneeId, Guid ownerId,
        string entityType, Guid entityId, DateTimeOffset? dueAt = null,
        ActivityPriority priority = ActivityPriority.Normal, string? description = null)
    {
        var activity = new TestActivity();
        activity.InitializeCore(Guid.NewGuid(), type, title, assigneeId, ownerId,
            entityType, entityId, dueAt, priority, description);
        return activity;
    }

    public void SetCallOutcome(string outcome) => CallOutcome = outcome;
}
