namespace Cheetah.Validation;

/// <summary>
/// Применяет наборы правил к набору полей. Не падает на первой ошибке — собирает все,
/// чтобы UI мог подсветить разом. Кросс-полевые правила видят весь словарь значений.
/// </summary>
public interface IValidationEngine
{
    ValueTask<ValidationResult> ValidateAsync(
        IReadOnlyList<FieldValidation> fields,
        IReadOnlyDictionary<string, object?> allValues,
        IServiceProvider services,
        CancellationToken ct = default);
}

/// <summary>
/// Описание валидации одного поля: путь, значение, набор правил.
/// </summary>
public sealed record FieldValidation(
    string FieldPath,
    object? Value,
    IReadOnlyList<IValidationRule> Rules);
