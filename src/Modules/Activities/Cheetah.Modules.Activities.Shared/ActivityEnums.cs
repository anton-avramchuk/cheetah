namespace Cheetah.Modules.Activities.Shared;

/// <summary>Вид активности.</summary>
public enum ActivityType
{
    Task = 0,
    Call = 1,
    Meeting = 2,
    Email = 3
}

/// <summary>Статус активности (участвует в конечном автомате).</summary>
public enum ActivityStatus
{
    Open = 0,
    InProgress = 1,
    Done = 2,
    Canceled = 3
}

/// <summary>Приоритет активности.</summary>
public enum ActivityPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
