namespace Cheetah.Modules.Activities.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов активности (не найдена, некорректные данные, недопустимый
/// переход). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class ActivityValidationException : Exception
{
    public ActivityValidationException(string message) : base(message) { }
}
