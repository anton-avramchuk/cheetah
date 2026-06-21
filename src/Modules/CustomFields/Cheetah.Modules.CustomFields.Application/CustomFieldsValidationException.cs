using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Application;

/// <summary>
/// Значения кастомных полей не прошли валидацию. Наследует <see cref="ArgumentException"/>, чтобы
/// глобальный обработчик отдал 400 Bad Request. Содержит все ошибки полей (не падает на первой).
/// </summary>
public sealed class CustomFieldsValidationException : ArgumentException
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public CustomFieldsValidationException(IReadOnlyList<ValidationError> errors)
        : base(BuildMessage(errors))
        => Errors = errors;

    private static string BuildMessage(IReadOnlyList<ValidationError> errors)
        => "Custom field validation failed: " +
           string.Join("; ", errors.Select(e => $"{e.FieldPath}: {e.Message}"));
}
