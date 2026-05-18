using System.Text.RegularExpressions;

namespace Cheetah.Validation.Rules;

/// <summary>
/// Строка матчится regex'у. ReDoS-защита через жёсткий MatchTimeout.
/// null-значения пропускаются.
/// </summary>
public sealed class PatternRule : IValidationRule
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(50);

    public string Pattern { get; init; } = "";

    public string RuleType => nameof(PatternRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        if (context.Value is null) return ValueTask.FromResult(ValidationOutcome.Valid);
        if (context.Value is not string s)
            return ValueTask.FromResult(ValidationOutcome.Invalid("pattern.not_string",
                $"{context.FieldPath} is not a string"));

        try
        {
            var regex = new Regex(Pattern, RegexOptions.CultureInvariant, MatchTimeout);
            return ValueTask.FromResult(regex.IsMatch(s)
                ? ValidationOutcome.Valid
                : ValidationOutcome.Invalid("pattern.mismatch", $"{context.FieldPath} does not match pattern"));
        }
        catch (ArgumentException)
        {
            return ValueTask.FromResult(ValidationOutcome.Invalid("pattern.invalid_regex",
                $"Pattern is not a valid regex"));
        }
        catch (RegexMatchTimeoutException)
        {
            return ValueTask.FromResult(ValidationOutcome.Invalid("pattern.timeout",
                $"Regex evaluation timed out"));
        }
    }
}
