using Cheetah.Core.Domain;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Участник события — логическая ссылка на пользователя Identity (<see cref="UserId"/>),
/// без FK через границу модуля. Часть агрегата <see cref="CalendarEvent"/>.
/// </summary>
public class EventAttendee : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public AttendeeRole Role { get; private set; }
    public AttendeeResponse Response { get; private set; }

    private EventAttendee() { } // EF

    internal static EventAttendee Create(Guid eventId, Guid userId, AttendeeRole role)
        => new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Role = role,
            Response = role == AttendeeRole.Organizer ? AttendeeResponse.Accepted : AttendeeResponse.NeedsAction
        };

    internal void Respond(AttendeeResponse response) => Response = response;
}
