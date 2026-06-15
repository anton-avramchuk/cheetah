namespace Cheetah.Modules.Leads.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов лида (не найден, дубль, недопустимый переход, не Qualified
/// при конвертации). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class LeadValidationException : Exception
{
    public LeadValidationException(string message) : base(message) { }
}
