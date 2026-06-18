namespace Cheetah.Modules.Booking.Shared;

/// <summary>Статус брони (участвует в конечном автомате).</summary>
public enum BookingStatus
{
    Confirmed = 0,
    Rescheduled = 1,
    Cancelled = 2,
    NoShow = 3,
    Completed = 4
}

/// <summary>Вид места встречи.</summary>
public enum LocationKind
{
    Video = 0,
    Phone = 1,
    InPerson = 2
}
