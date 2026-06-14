namespace Cheetah.Modules.Calendar.Application.Exceptions;

/// <summary>Нарушение бизнес-правил Calendar (несуществующий тип привязки, неизвестное событие и т.п.).</summary>
public sealed class CalendarValidationException : Exception
{
    public CalendarValidationException(string message) : base(message) { }
}
