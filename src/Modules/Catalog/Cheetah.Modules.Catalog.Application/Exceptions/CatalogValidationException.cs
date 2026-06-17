namespace Cheetah.Modules.Catalog.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов каталога (не найдено, дубликат артикула, некорректные
/// данные). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class CatalogValidationException : Exception
{
    public CatalogValidationException(string message) : base(message) { }
}
