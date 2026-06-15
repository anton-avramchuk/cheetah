using Cheetah.Core.Domain;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Activities.DomainEvents;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат активности. Шаблонный модуль не инстанцирует его сам — наследник
/// объявляет конкретный <c>sealed class Activity : ActivityBase</c> со своей фабрикой (через
/// <see cref="InitializeCore"/>) и доп. полями. Это и есть точка расширяемости сущности.
/// <para>
/// Статус (<see cref="ActivityStatus"/>) участвует в конечном автомате через
/// <see cref="IStateMachineEntity{TState}"/>. Привязка к произвольной сущности CRM — полиморфная
/// <c>(EntityType, EntityId)</c>.
/// </para>
/// </summary>
public abstract class ActivityBase : AggregateRoot<Guid>,
    IStateMachineEntity<ActivityStatus>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    private readonly List<ActivityReminder> _reminders = new();

    public ActivityType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public ActivityStatus Status { get; private set; }
    public ActivityPriority Priority { get; private set; }
    public Guid AssigneeId { get; private set; }
    public Guid OwnerId { get; private set; }

    /// <summary>Полиморфная привязка к произвольной сущности CRM (deal/customer/contact/lead/…).</summary>
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    public DateTimeOffset? DueAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? Result { get; private set; }

    /// <summary>Опциональная ссылка на событие Calendar (для активности типа Meeting).</summary>
    public Guid? CalendarEventId { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb). Полноценно — модуль Custom Fields.</summary>
    public string? Attributes { get; private set; }

    public IReadOnlyList<ActivityReminder> Reminders => _reminders;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    /// <inheritdoc />
    public ActivityStatus State => Status;

    protected ActivityBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты новой активности и доменное событие создания. Вызывается фабрикой
    /// наследника (замена <c>new</c> абстрактной сущности).
    /// </summary>
    protected void InitializeCore(
        Guid id, ActivityType type, string title, Guid assigneeId, Guid ownerId,
        string entityType, Guid entityId, DateTimeOffset? dueAt = null,
        ActivityPriority priority = ActivityPriority.Normal, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        Id = id;
        Type = type;
        Title = title.Trim();
        Status = ActivityStatus.Open;
        Priority = priority;
        AssigneeId = assigneeId;
        OwnerId = ownerId;
        EntityType = entityType.Trim();
        EntityId = entityId;
        DueAt = dueAt;
        Description = description;

        AddDomainEvent(new ActivityCreatedIntegrationEvent(Id, EntityType, EntityId, AssigneeId, DueAt));
    }

    public ActivityReminder AddReminder(TimeSpan offsetBeforeDue, string channel)
    {
        if (DueAt is null)
            throw new InvalidOperationException("A reminder requires DueAt to be set.");

        var reminder = ActivityReminder.Create(Id, offsetBeforeDue, channel);
        _reminders.Add(reminder);
        return reminder;
    }

    /// <summary>Обновление базовых полей. Доп. поля наследника обновляет он сам (переопределив хендлер).</summary>
    public virtual void Update(string title, string? description, ActivityPriority priority, DateTimeOffset? dueAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
        Description = description;
        Priority = priority;
        DueAt = dueAt;
    }

    public virtual void Start()
    {
        if (Status == ActivityStatus.Open)
            Status = ActivityStatus.InProgress;
    }

    public virtual void Complete(Guid completedBy, string? result = null)
    {
        if (Status is ActivityStatus.Done or ActivityStatus.Canceled)
            return;

        Status = ActivityStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
        Result = result;
        AddDomainEvent(new ActivityCompletedIntegrationEvent(Id, completedBy));
    }

    public virtual void Cancel()
    {
        if (Status == ActivityStatus.Canceled)
            return;

        Status = ActivityStatus.Canceled;
        AddDomainEvent(new ActivityCanceledIntegrationEvent(Id));
    }

    public virtual void Reassign(Guid newAssigneeId)
    {
        if (newAssigneeId == AssigneeId)
            return;

        var old = AssigneeId;
        AssigneeId = newAssigneeId;
        AddDomainEvent(new ActivityReassignedIntegrationEvent(Id, old, newAssigneeId));
    }

    public void LinkCalendarEvent(Guid calendarEventId) => CalendarEventId = calendarEventId;

    public void SetAttributes(string? attributes) => Attributes = attributes;

    /// <summary>Вызывается фоновой задачей скана просрочек. Публикует событие только при переходе в просрочку.</summary>
    public bool TryMarkOverdue(DateTimeOffset now)
    {
        if (Status == ActivityStatus.Open && DueAt is { } due && due < now)
        {
            AddDomainEvent(new ActivityOverdueIntegrationEvent(Id, AssigneeId));
            return true;
        }

        return false;
    }
}
