namespace Cheetah.Modules.Booking.Shared;

/// <summary>
/// Общие константы шаблонного модуля Booking: имя БД-подключения, ограничения длин, дефолтные имена
/// таблиц/схемы и префиксы маршрутов. Используются абстрактными базами как значения по умолчанию,
/// которые наследник может переопределить.
/// </summary>
public static class BookingConstants
{
    public const string ConnectionStringName = "Booking";

    public const int MaxNameLength = 200;
    public const int MaxSlugLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxTimeZoneLength = 64;
    public const int MaxManageTokenLength = 64;
    public const int MaxReasonLength = 1000;

    public const string DefaultSchema = "booking";
    public const string BookingsTableName = "Bookings";
    public const string BookingTypesTableName = "BookingTypes";
    public const string SchedulesTableName = "AvailabilitySchedules";

    public const string PublicRoutePrefix = "api/public/booking";
    public const string BookingTypesRoutePrefix = "api/booking-types";
    public const string AvailabilityRoutePrefix = "api/availability";
    public const string BookingsRoutePrefix = "api/bookings";
}
