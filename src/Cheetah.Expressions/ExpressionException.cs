namespace Cheetah.Expressions;

/// <summary>
/// Базовое исключение движка выражений. Бросается только в исключительных ситуациях
/// (внутренние сбои evaluator'а). Ошибки выражений и нарушения лимитов возвращаются
/// как ExpressionResult.Fail / ExpressionParseResult.Fail.
/// </summary>
public class ExpressionException : Exception
{
    public ExpressionException(string message) : base(message) { }
    public ExpressionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Выражение превысило один из лимитов безопасности (длина, глубина, время выполнения).
/// </summary>
public sealed class ExpressionLimitExceededException : ExpressionException
{
    public string Limit { get; }

    public ExpressionLimitExceededException(string limit, string message)
        : base(message)
    {
        Limit = limit;
    }
}
