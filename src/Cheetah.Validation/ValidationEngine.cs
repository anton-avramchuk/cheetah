namespace Cheetah.Validation;

public sealed class ValidationEngine : IValidationEngine
{
    public async ValueTask<ValidationResult> ValidateAsync(
        IReadOnlyList<FieldValidation> fields,
        IReadOnlyDictionary<string, object?> allValues,
        IServiceProvider services,
        CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        foreach (var field in fields)
        {
            var ctx = new ValidationContext(field.FieldPath, field.Value, allValues, services);
            foreach (var rule in field.Rules)
            {
                ct.ThrowIfCancellationRequested();
                var outcome = await rule.ValidateAsync(ctx, ct);
                if (!outcome.IsValid)
                {
                    errors.Add(new ValidationError(
                        field.FieldPath,
                        outcome.Code ?? rule.RuleType,
                        outcome.Message ?? $"{rule.RuleType} violated"));
                }
            }
        }

        return errors.Count == 0 ? ValidationResult.Valid : ValidationResult.Failed(errors);
    }
}
