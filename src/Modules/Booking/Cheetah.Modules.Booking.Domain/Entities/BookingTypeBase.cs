using Cheetah.Core.Domain;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Domain.Entities;

/// <summary>
/// Абстрактный агрегат «тип встречи» (публичная страница записи host'а). Шаблонный модуль не
/// инстанцирует его сам — наследник объявляет <c>sealed class BookingType : BookingTypeBase</c> со
/// своей фабрикой (через <see cref="InitializeCore"/>) и доп. полями. Точка расширяемости сущности.
/// </summary>
public abstract class BookingTypeBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public Guid HostUserId { get; private set; }

    /// <summary>Уникальный сегмент публичного URL страницы записи.</summary>
    public string Slug { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int DurationMinutes { get; private set; }
    public LocationKind LocationKind { get; private set; }
    public string? LocationDetails { get; private set; }

    /// <summary>Буфер до встречи (минуты): расширяет проверяемый интервал занятости, но не сам слот.</summary>
    public int BufferBeforeMinutes { get; private set; }

    /// <summary>Буфер после встречи (минуты).</summary>
    public int BufferAfterMinutes { get; private set; }

    /// <summary>Минимальный запас по времени до встречи (минуты) — нельзя бронировать «прямо сейчас».</summary>
    public int MinNoticeMinutes { get; private set; }

    /// <summary>Горизонт планирования (дни) — насколько вперёд видны слоты.</summary>
    public int MaxAdvanceDays { get; private set; }

    /// <summary>Шаг сетки слотов (минуты). <c>null</c> → шаг равен длительности встречи.</summary>
    public int? SlotStepMinutes { get; private set; }

    public string? Color { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb). Полноценно — модуль Custom Fields.</summary>
    public string? Attributes { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected BookingTypeBase() { } // EF + наследник

    /// <summary>Заводит инварианты нового типа встречи. Вызывается фабрикой наследника (замена <c>new</c>).</summary>
    protected void InitializeCore(
        Guid id, Guid hostUserId, string slug, string name, int durationMinutes, LocationKind locationKind,
        int maxAdvanceDays, int minNoticeMinutes, int bufferBeforeMinutes = 0, int bufferAfterMinutes = 0,
        int? slotStepMinutes = null, string? locationDetails = null, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be positive.");
        if (maxAdvanceDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAdvanceDays), "Advance horizon must be positive.");
        if (bufferBeforeMinutes < 0 || bufferAfterMinutes < 0 || minNoticeMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minNoticeMinutes), "Offsets must be non-negative.");
        if (slotStepMinutes is <= 0)
            throw new ArgumentOutOfRangeException(nameof(slotStepMinutes), "Slot step must be positive.");

        Id = id;
        HostUserId = hostUserId;
        Slug = slug.Trim().ToLowerInvariant();
        Name = name.Trim();
        DurationMinutes = durationMinutes;
        LocationKind = locationKind;
        LocationDetails = locationDetails;
        BufferBeforeMinutes = bufferBeforeMinutes;
        BufferAfterMinutes = bufferAfterMinutes;
        MinNoticeMinutes = minNoticeMinutes;
        MaxAdvanceDays = maxAdvanceDays;
        SlotStepMinutes = slotStepMinutes;
        Color = color;
        IsActive = true;
    }

    /// <summary>Эффективный шаг сетки слотов: <see cref="SlotStepMinutes"/> либо длительность.</summary>
    public int EffectiveSlotStepMinutes => SlotStepMinutes ?? DurationMinutes;

    public virtual void Update(
        string name, int durationMinutes, LocationKind locationKind, string? locationDetails,
        int bufferBeforeMinutes, int bufferAfterMinutes, int minNoticeMinutes, int maxAdvanceDays,
        int? slotStepMinutes, string? color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be positive.");

        Name = name.Trim();
        DurationMinutes = durationMinutes;
        LocationKind = locationKind;
        LocationDetails = locationDetails;
        BufferBeforeMinutes = bufferBeforeMinutes;
        BufferAfterMinutes = bufferAfterMinutes;
        MinNoticeMinutes = minNoticeMinutes;
        MaxAdvanceDays = maxAdvanceDays;
        SlotStepMinutes = slotStepMinutes;
        Color = color;
    }

    public virtual void Deactivate() => IsActive = false;

    public void SetAttributes(string? attributes) => Attributes = attributes;
}
