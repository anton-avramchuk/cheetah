namespace Cheetah.Expressions;

/// <summary>
/// Результат вычисления выражения. Success=false означает, что выражение не вычислилось:
/// см. Error для деталей. Value валиден только при Success=true.
/// </summary>
public sealed record ExpressionResult<T>(bool Success, T? Value, string? Error)
{
    public static ExpressionResult<T> Ok(T value) => new(true, value, null);
    public static ExpressionResult<T> Fail(string error) => new(false, default, error);
}
