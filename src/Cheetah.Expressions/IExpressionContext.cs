namespace Cheetah.Expressions;

/// <summary>
/// Фасад над контекстом выражения. Поддерживает dotted paths: TryGet("document.amount").
/// Не обязателен для evaluator'а — это удобная обёртка для построения контекста из кода.
/// </summary>
public interface IExpressionContext
{
    /// <summary>
    /// Получить значение по dotted-path. Например: "document.amount", "user.role".
    /// </summary>
    bool TryGet(string path, out object? value);

    /// <summary>
    /// Снимок контекста как плоского словаря (с dotted-ключами).
    /// </summary>
    IReadOnlyDictionary<string, object?> ToDictionary();
}
