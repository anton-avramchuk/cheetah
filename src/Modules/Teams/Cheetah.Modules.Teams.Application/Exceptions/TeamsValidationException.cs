namespace Cheetah.Modules.Teams.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов модуля Teams (не найдено, дубликат имени, некорректные
/// данные). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class TeamsValidationException : Exception
{
    public TeamsValidationException(string message) : base(message) { }
}
