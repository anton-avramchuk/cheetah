namespace Cheetah.Modules.Customer.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов клиента (не найден, некорректные данные и т.п.).
/// Маппится в API на 400 Bad Request.
/// </summary>
public sealed class CustomerValidationException : Exception
{
    public CustomerValidationException(string message) : base(message) { }
}
