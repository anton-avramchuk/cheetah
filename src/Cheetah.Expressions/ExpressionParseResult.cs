namespace Cheetah.Expressions;

/// <summary>
/// Результат разбора выражения. Используется UI-редактором для подсветки ошибок синтаксиса
/// и предпросмотра набора переменных, которые ожидает выражение.
/// </summary>
public sealed record ExpressionParseResult(
    bool Valid,
    string? Error,
    IReadOnlyList<string> ReferencedVariables)
{
    public static ExpressionParseResult Ok(IReadOnlyList<string> variables)
        => new(true, null, variables);

    public static ExpressionParseResult Fail(string error)
        => new(false, error, Array.Empty<string>());
}
