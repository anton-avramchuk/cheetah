using Cheetah.Expressions;

namespace Cheetah.Validation.Rules;

public enum CompareOperator { Eq, Ne, Gt, Ge, Lt, Le }

/// <summary>
/// Сравнение текущего поля с другим полем по dotted-path.
/// Пример: EndDate &gt; StartDate.
/// null-значения с обеих сторон → Valid; одно null → Invalid.
/// </summary>
public sealed class CompareRule : IValidationRule
{
    public string OtherFieldPath { get; init; } = "";
    public CompareOperator Operator { get; init; }

    public string RuleType => nameof(CompareRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        var ctxAll = new DictionaryExpressionContext(context.AllValues);
        ctxAll.TryGet(OtherFieldPath, out var other);

        if (context.Value is null && other is null)
            return ValueTask.FromResult(ValidationOutcome.Valid);

        if (context.Value is null || other is null)
            return ValueTask.FromResult(ValidationOutcome.Invalid("compare.null_mismatch",
                $"Cannot compare {context.FieldPath} and {OtherFieldPath}: one is null"));

        if (context.Value is not IComparable cmp)
            return ValueTask.FromResult(ValidationOutcome.Invalid("compare.not_comparable",
                $"{context.FieldPath} is not IComparable"));

        int cmpResult;
        try
        {
            cmpResult = cmp.CompareTo(other);
        }
        catch (ArgumentException)
        {
            return ValueTask.FromResult(ValidationOutcome.Invalid("compare.type_mismatch",
                $"Cannot compare {context.FieldPath} ({context.Value.GetType().Name}) with {OtherFieldPath} ({other.GetType().Name})"));
        }

        var ok = Operator switch
        {
            CompareOperator.Eq => cmpResult == 0,
            CompareOperator.Ne => cmpResult != 0,
            CompareOperator.Gt => cmpResult > 0,
            CompareOperator.Ge => cmpResult >= 0,
            CompareOperator.Lt => cmpResult < 0,
            CompareOperator.Le => cmpResult <= 0,
            _ => false
        };

        return ValueTask.FromResult(ok
            ? ValidationOutcome.Valid
            : ValidationOutcome.Invalid("compare.failed",
                $"{context.FieldPath} {Operator} {OtherFieldPath} is false"));
    }
}
