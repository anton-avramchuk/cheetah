namespace Cheetah.Modules.Booking.Application.Exceptions;

/// <summary>Нарушение бизнес-правила брони (404/400-класс): не найдено, неверный запрос.</summary>
public sealed class BookingValidationException : Exception
{
    public BookingValidationException(string message) : base(message) { }
}

/// <summary>Конфликт слота (409): слот занят/бронируется другим, гонка на подтверждении.</summary>
public sealed class BookingConflictException : Exception
{
    public BookingConflictException(string message) : base(message) { }
}
