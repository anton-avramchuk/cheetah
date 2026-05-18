namespace Cheetah.Validation.Rules;

/// <summary>
/// Значение не null и (для строк) не пустое/whitespace.
/// </summary>
public sealed class RequiredRule : IValidationRule
{
    public string RuleType => nameof(RequiredRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        var ok = context.Value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            _ => true
        };

        return ValueTask.FromResult(ok
            ? ValidationOutcome.Valid
            : ValidationOutcome.Invalid("required", $"{context.FieldPath} is required"));
    }
}
