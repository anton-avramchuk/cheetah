using Cheetah.Core.Events;

namespace Cheetah.Modules.Booking.DomainEvents;

/// <summary>Бронь подтверждена (создана). Потребители: Notification (письма/напоминания), Leads, Activities.</summary>
public record BookingConfirmedIntegrationEvent(
    Guid BookingId, Guid BookingTypeId, Guid HostUserId,
    DateTimeOffset StartUtc, DateTimeOffset EndUtc, string InviteeName, string InviteeEmail) : EventBase;

/// <summary>Бронь перенесена на новое время.</summary>
public record BookingRescheduledIntegrationEvent(
    Guid BookingId, DateTimeOffset StartUtc, DateTimeOffset EndUtc) : EventBase;

/// <summary>Бронь отменена (host'ом или invitee).</summary>
public record BookingCancelledIntegrationEvent(Guid BookingId, string Reason, bool ByInvitee) : EventBase;

/// <summary>Invitee не явился на встречу.</summary>
public record BookingNoShowIntegrationEvent(Guid BookingId) : EventBase;
