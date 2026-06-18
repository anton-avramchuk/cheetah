using System.Security.Cryptography;
using Cheetah.Core.Domain;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Booking.DomainEvents;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Domain.Entities;

/// <summary>
/// Абстрактный агрегат «бронь». Шаблонный модуль не инстанцирует его сам — наследник объявляет
/// <c>sealed class Booking : BookingBase</c> со своей фабрикой (через <see cref="InitializeCore"/>)
/// и доп. полями. Статус (<see cref="BookingStatus"/>) участвует в конечном автомате.
/// </summary>
public abstract class BookingBase : AggregateRoot<Guid>,
    IStateMachineEntity<BookingStatus>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<BookingAnswer> _answers = new();

    public Guid BookingTypeId { get; private set; }
    public Guid HostUserId { get; private set; }
    public string InviteeName { get; private set; } = null!;
    public string InviteeEmail { get; private set; } = null!;
    public string? InviteePhone { get; private set; }
    public string InviteeTimeZone { get; private set; } = "UTC";
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public BookingStatus Status { get; private set; }

    public Guid? CalendarEventId { get; private set; }
    public Guid? CreatedLeadId { get; private set; }
    public Guid? CreatedActivityId { get; private set; }

    /// <summary>Одноразовый неугадываемый токен управления бронью (перенос/отмена invitee без логина).</summary>
    public string ManageToken { get; private set; } = null!;
    public string? CancelReason { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb).</summary>
    public string? Attributes { get; private set; }

    public IReadOnlyList<BookingAnswer> Answers => _answers;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public BookingStatus State => Status;

    protected BookingBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты новой брони, генерирует <see cref="ManageToken"/> и доменное событие
    /// подтверждения. Вызывается фабрикой наследника (замена <c>new</c> + <c>Reserve</c>).
    /// </summary>
    protected void InitializeCore(
        Guid id, BookingTypeBase type, DateTimeOffset startUtc,
        string inviteeName, string inviteeEmail, string inviteeTimeZone, string? inviteePhone,
        IEnumerable<BookingAnswer> answers)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(inviteeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(inviteeEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(inviteeTimeZone);

        Id = id;
        BookingTypeId = type.Id;
        HostUserId = type.HostUserId;
        StartUtc = startUtc;
        EndUtc = startUtc.AddMinutes(type.DurationMinutes);
        InviteeName = inviteeName.Trim();
        InviteeEmail = inviteeEmail.Trim();
        InviteePhone = inviteePhone;
        InviteeTimeZone = inviteeTimeZone.Trim();
        Status = BookingStatus.Confirmed;
        ManageToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _answers.AddRange(answers);

        AddDomainEvent(new BookingConfirmedIntegrationEvent(
            Id, type.Id, type.HostUserId, StartUtc, EndUtc, InviteeName, InviteeEmail));
    }

    public void AttachCalendarEvent(Guid calendarEventId) => CalendarEventId = calendarEventId;
    public void AttachLead(Guid leadId) => CreatedLeadId = leadId;
    public void AttachActivity(Guid activityId) => CreatedActivityId = activityId;
    public void SetAttributes(string? attributes) => Attributes = attributes;

    public virtual void Reschedule(DateTimeOffset newStartUtc, int durationMinutes)
    {
        EnsureActive();
        StartUtc = newStartUtc;
        EndUtc = newStartUtc.AddMinutes(durationMinutes);
        Status = BookingStatus.Rescheduled;
        AddDomainEvent(new BookingRescheduledIntegrationEvent(Id, StartUtc, EndUtc));
    }

    public virtual void Cancel(string reason, bool byInvitee)
    {
        EnsureActive();
        Status = BookingStatus.Cancelled;
        CancelReason = reason;
        AddDomainEvent(new BookingCancelledIntegrationEvent(Id, reason, byInvitee));
    }

    public virtual void MarkNoShow()
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Rescheduled)
        {
            Status = BookingStatus.NoShow;
            AddDomainEvent(new BookingNoShowIntegrationEvent(Id));
        }
    }

    public virtual void Complete()
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Rescheduled)
            Status = BookingStatus.Completed;
    }

    /// <summary>Активна ли бронь (не в терминальном статусе).</summary>
    public bool IsActive => Status is BookingStatus.Confirmed or BookingStatus.Rescheduled;

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Booking {Id} is not active ({Status}).");
    }
}

/// <summary>Ответ invitee на intake-вопрос страницы записи. Дитя агрегата брони.</summary>
public sealed class BookingAnswer : Entity<Guid>
{
    public Guid BookingId { get; private set; }
    public string Question { get; private set; } = null!;
    public string? Value { get; private set; }

    private BookingAnswer() { } // EF

    public static BookingAnswer Create(string question, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        return new BookingAnswer { Id = Guid.NewGuid(), Question = question.Trim(), Value = value };
    }
}
