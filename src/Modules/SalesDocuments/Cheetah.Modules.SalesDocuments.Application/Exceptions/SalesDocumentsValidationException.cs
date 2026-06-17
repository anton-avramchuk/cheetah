namespace Cheetah.Modules.SalesDocuments.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов модуля документов (не найдено, дубликат номера, некорректные
/// данные, недопустимый переход). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class SalesDocumentsValidationException : Exception
{
    public SalesDocumentsValidationException(string message) : base(message) { }
}
