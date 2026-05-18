using System.Globalization;

namespace Cheetah.Validation.Rules;

/// <summary>
/// Дата в диапазоне [Min, Max]. Принимает DateTimeOffset/DateTime/строковую ISO-8601.
/// null-значения пропускаются.
/// </summary>
public sealed class DateRangeRule : IValidationRule
{
    public DateTimeOffset? Min { get; init; }
    public DateTimeOffset? Max { get; init; }

    public string RuleType => nameof(DateRangeRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        if (context.Value is null) return ValueTask.FromResult(ValidationOutcome.Valid);

        if (!TryToDate(context.Value, out var v))
            return ValueTask.FromResult(ValidationOutcome.Invalid("date_range.not_date",
                $"{context.FieldPath} is not a date"));

        if (Min.HasValue && v < Min.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("date_range.before_min",
                $"{context.FieldPath} < {Min.Value:O}"));

        if (Max.HasValue && v > Max.Value)
            return ValueTask.FromResult(ValidationOutcome.Invalid("date_range.after_max",
                $"{context.FieldPath} > {Max.Value:O}"));

        return ValueTask.FromResult(ValidationOutcome.Valid);
    }

    private static bool TryToDate(object value, out DateTimeOffset result)
    {
        switch (value)
        {
            case DateTimeOffset dto: result = dto; return true;
            case DateTime dt:
                result = dt.Kind == DateTimeKind.Unspecified
                    ? new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc))
                    : new DateTimeOffset(dt);
                return true;
            case string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed):
                result = parsed; return true;
            default:
                result = default;
                return false;
        }
    }
}
