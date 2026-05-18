using Cheetah.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Validation.Rules;

/// <summary>
/// Required, но только если Expression возвращает true. Полезно для условных полей:
/// "дата подписания обязательна, если статус = Подписан".
/// </summary>
public sealed class RequiredIfRule : IValidationRule
{
    public string Expression { get; init; } = "";

    public string RuleType => nameof(RequiredIfRule);

    public async ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        var evaluator = context.Services.GetService<IExpressionEvaluator>();
        if (evaluator is null) return ValidationOutcome.Valid;

        var ctxData = new Dictionary<string, object?>(context.AllValues) { ["value"] = context.Value };
        var conditionTrue = await evaluator.EvaluateBooleanAsync(Expression, ctxData, ct);
        if (!conditionTrue) return ValidationOutcome.Valid;

        var ok = context.Value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            _ => true
        };

        return ok
            ? ValidationOutcome.Valid
            : ValidationOutcome.Invalid("required_if", $"{context.FieldPath} is required when condition is true");
    }
}
