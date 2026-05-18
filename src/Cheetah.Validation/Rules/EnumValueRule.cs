namespace Cheetah.Validation.Rules;

/// <summary>
/// Значение содержится в списке AllowedValues. Сравнение через ToString() — работает
/// для строк, чисел, Guid, enum'ов и любых других "перечислимых" типов.
/// null-значения пропускаются.
/// </summary>
public sealed class EnumValueRule : IValidationRule
{
    public IReadOnlyList<string> AllowedValues { get; init; } = Array.Empty<string>();

    public string RuleType => nameof(EnumValueRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        if (context.Value is null) return ValueTask.FromResult(ValidationOutcome.Valid);

        var asString = context.Value.ToString() ?? "";
        return ValueTask.FromResult(AllowedValues.Contains(asString, StringComparer.Ordinal)
            ? ValidationOutcome.Valid
            : ValidationOutcome.Invalid("enum.not_allowed",
                $"{context.FieldPath} = '{asString}' is not in allowed values"));
    }
}
