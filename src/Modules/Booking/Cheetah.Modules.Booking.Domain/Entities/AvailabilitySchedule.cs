using Cheetah.Core.Domain;

namespace Cheetah.Modules.Booking.Domain.Entities;

/// <summary>
/// Абстрактный агрегат недельной доступности host'а: рабочие окна по дням недели + исключения на
/// конкретные даты, в таймзоне host'а. Наследник объявляет <c>sealed class AvailabilitySchedule :
/// AvailabilityScheduleBase</c>. Дочерние сущности (правила/исключения) — конкретные, не расширяются.
/// </summary>
public abstract class AvailabilityScheduleBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<WeeklyAvailabilityRule> _weeklyRules = new();
    private readonly List<AvailabilityDateOverride> _dateOverrides = new();

    public Guid HostUserId { get; private set; }

    /// <summary>IANA/Windows-идентификатор таймзоны host'а (окна заданы в этой таймзоне).</summary>
    public string TimeZoneId { get; private set; } = "UTC";

    public IReadOnlyList<WeeklyAvailabilityRule> WeeklyRules => _weeklyRules;
    public IReadOnlyList<AvailabilityDateOverride> DateOverrides => _dateOverrides;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected AvailabilityScheduleBase() { } // EF + наследник

    protected void InitializeCore(Guid id, Guid hostUserId, string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        Id = id;
        HostUserId = hostUserId;
        TimeZoneId = timeZoneId.Trim();
    }

    public void SetTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        TimeZoneId = timeZoneId.Trim();
    }

    public WeeklyAvailabilityRule AddWeeklyRule(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Availability window end must be after start.", nameof(endTime));

        var rule = WeeklyAvailabilityRule.Create(Id, dayOfWeek, startTime, endTime);
        _weeklyRules.Add(rule);
        return rule;
    }

    public AvailabilityDateOverride AddDateOverride(
        DateOnly date, bool isUnavailable, TimeOnly? startTime, TimeOnly? endTime)
    {
        if (!isUnavailable && startTime is { } s && endTime is { } e && e <= s)
            throw new ArgumentException("Override window end must be after start.", nameof(endTime));

        var ovr = AvailabilityDateOverride.Create(Id, date, isUnavailable, startTime, endTime);
        _dateOverrides.RemoveAll(o => o.Date == date);
        _dateOverrides.Add(ovr);
        return ovr;
    }

    /// <summary>Полная замена правил/исключений (upsert недельной доступности).</summary>
    public void ReplaceRules(
        IEnumerable<(DayOfWeek Day, TimeOnly Start, TimeOnly End)> weekly,
        IEnumerable<(DateOnly Date, bool IsUnavailable, TimeOnly? Start, TimeOnly? End)> overrides)
    {
        _weeklyRules.Clear();
        _dateOverrides.Clear();
        foreach (var w in weekly)
            AddWeeklyRule(w.Day, w.Start, w.End);
        foreach (var o in overrides)
            AddDateOverride(o.Date, o.IsUnavailable, o.Start, o.End);
    }
}

/// <summary>Рабочее окно в дне недели (локальное время host'а). Дитя агрегата доступности.</summary>
public sealed class WeeklyAvailabilityRule : Entity<Guid>
{
    public Guid ScheduleId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private WeeklyAvailabilityRule() { } // EF

    internal static WeeklyAvailabilityRule Create(Guid scheduleId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        => new()
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime
        };
}

/// <summary>
/// Исключение доступности на конкретную дату: либо день недоступен целиком
/// (<see cref="IsUnavailable"/>), либо задаёт особое окно (<see cref="StartTime"/>/<see cref="EndTime"/>),
/// переопределяющее недельное правило. Дитя агрегата доступности.
/// </summary>
public sealed class AvailabilityDateOverride : Entity<Guid>
{
    public Guid ScheduleId { get; private set; }
    public DateOnly Date { get; private set; }
    public bool IsUnavailable { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }

    private AvailabilityDateOverride() { } // EF

    internal static AvailabilityDateOverride Create(
        Guid scheduleId, DateOnly date, bool isUnavailable, TimeOnly? startTime, TimeOnly? endTime)
        => new()
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            Date = date,
            IsUnavailable = isUnavailable,
            StartTime = isUnavailable ? null : startTime,
            EndTime = isUnavailable ? null : endTime
        };
}
