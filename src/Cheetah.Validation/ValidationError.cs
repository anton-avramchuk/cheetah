namespace Cheetah.Validation;

/// <summary>
/// Ошибка валидации поля. Привязана к конкретному FieldPath.
/// </summary>
public sealed record ValidationError(string FieldPath, string Code, string Message);

/// <summary>
/// Агрегированный результат валидации. Содержит ВСЕ обнаруженные ошибки
/// (не падает на первой), чтобы UI мог подсветить все поля разом.
/// </summary>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    public static readonly ValidationResult Valid = new(true, Array.Empty<ValidationError>());

    public static ValidationResult Failed(IReadOnlyList<ValidationError> errors)
        => new(errors.Count == 0, errors);
}
