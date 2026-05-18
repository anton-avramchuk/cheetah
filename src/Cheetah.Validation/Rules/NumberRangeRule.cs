using System.Globalization;

namespace Cheetah.Validation.Rules;

/// <summary>
/// Число в диапазоне [Min, Max]. Работает с любым числовым типом (int/long/decimal/double/...).
/// null-значения пропускаются.
/// </summary>
public sealed class NumberRangeRule : IValidationRule
{
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }

    public string RuleType => nameof(NumberRangeRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        if (context.Value is null) return ValueTask.FromResult(ValidationOutcome.Valid);

        if (!TryToDecimal(context.Value, out var v))
            return ValueTask.FromResult(ValidationOutcome.Invalid("number_range.not_number",
                $"{context.FieldPath} is not a number"));

        if (Min.HasValue && v < Min.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("number_range.below_min",
                $"{context.FieldPath} = {v} < min {Min.Value}"));

        if (Max.HasValue && v > Max.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("number_range.above_max",
                $"{context.FieldPath} = {v} > max {Max.Value}"));

        return ValueTask.FromResult(ValidationOutcome.Valid);
    }

    private static bool TryToDecimal(object value, out decimal result)
    {
        switch (value)
        {
            case decimal d: result = d; return true;
            case double db: result = (decimal)db; return true;
            case float f: result = (decimal)f; return true;
            case long l: result = l; return true;
            case int i: result = i; return true;
            case short sh: result = sh; return true;
            case byte b: result = b; return true;
            case string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed; return true;
            default:
                result = 0;
                return false;
        }
    }
}
