namespace Cheetah.Expressions;

/// <summary>
/// Движок выражений: парсит и вычисляет сериализованные выражения над словарём переменных.
/// Реализации должны быть thread-safe и stateless (Singleton-регистрация).
/// </summary>
public interface IExpressionEvaluator
{
    /// <summary>
    /// Вычисляет выражение с приведением результата к типу T.
    /// </summary>
    ValueTask<ExpressionResult<T>> EvaluateAsync<T>(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    /// <summary>
    /// Удобная перегрузка для условий: возвращает false если выражение не вычислилось или
    /// его результат не bool. Используется в guard'ах переходов и правилах валидации.
    /// </summary>
    ValueTask<bool> EvaluateBooleanAsync(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    /// <summary>
    /// Разбор выражения без выполнения. Возвращает синтаксические ошибки и список переменных,
    /// которые выражение читает (для UI-редактора).
    /// </summary>
    ExpressionParseResult Parse(string expression);
}
