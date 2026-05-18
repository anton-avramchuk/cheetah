using Cheetah.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Validation.Rules;

/// <summary>
/// Произвольное JsonLogic-выражение в качестве правила. Выражение получает значение
/// текущего поля в контексте как "value" + все остальные поля (плоско).
/// Если evaluator не зарегистрирован — правило считается Valid (no-op).
/// </summary>
public sealed class ExpressionRule : IValidationRule
{
    public string Expression { get; init; } = "";

    /// <summary>Сообщение об ошибке, если выражение вернуло false.</summary>
    public string? Message { get; init; }

    public string RuleType => nameof(ExpressionRule);

    public async ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        var evaluator = context.Services.GetService<IExpressionEvaluator>();
        if (evaluator is null) return ValidationOutcome.Valid;

        var ctxData = new Dictionary<string, object?>(context.AllValues) { ["value"] = context.Value };
        var ok = await evaluator.EvaluateBooleanAsync(Expression, ctxData, ct);

        return ok
            ? ValidationOutcome.Valid
            : ValidationOutcome.Invalid("expression.failed",
                Message ?? $"Expression validation failed for {context.FieldPath}");
    }
}
