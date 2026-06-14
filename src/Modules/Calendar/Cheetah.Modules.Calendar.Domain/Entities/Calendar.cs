using Cheetah.Core.Domain;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Календарь — контейнер событий. Может быть личным, привязанным к сущности или общим.
/// Именован <c>Calendar</c>; во избежание конфликта с пространством имён к сущности
/// обращаемся по полному имени там, где это требуется.
/// </summary>
public class Calendar : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public CalendarType Type { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string DefaultTimeZoneId { get; private set; } = "UTC";
    public string? Color { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Calendar() { } // EF

    public static Calendar Create(string name, CalendarType type, Guid ownerUserId, string defaultTimeZoneId, string? color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Calendar
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            OwnerUserId = ownerUserId,
            DefaultTimeZoneId = string.IsNullOrWhiteSpace(defaultTimeZoneId) ? "UTC" : defaultTimeZoneId,
            Color = color
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void ChangeColor(string? color) => Color = color;
}
