namespace Cheetah.Validation.Rules;

/// <summary>
/// Длина строки в диапазоне [Min, Max]. null-значения пропускаются (для них используйте RequiredRule).
/// Если задано только Min или только Max — проверяется только эта граница.
/// </summary>
public sealed class StringLengthRule : IValidationRule
{
    public int? Min { get; init; }
    public int? Max { get; init; }

    public string RuleType => nameof(StringLengthRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        if (context.Value is null) return ValueTask.FromResult(ValidationOutcome.Valid);
        if (context.Value is not string s)
            return ValueTask.FromResult(ValidationOutcome.Invalid("string_length.not_string",
                $"{context.FieldPath} is not a string"));

        if (Min.HasValue && s.Length < Min.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("string_length.too_short",
                $"{context.FieldPath} length {s.Length} < min {Min.Value}"));

        if (Max.HasValue && s.Length > Max.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("string_length.too_long",
                $"{context.FieldPath} length {s.Length} > max {Max.Value}"));

        return ValueTask.FromResult(ValidationOutcome.Valid);
    }
}
