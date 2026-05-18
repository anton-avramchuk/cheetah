namespace Cheetah.Expressions;

/// <summary>
/// Простая реализация IExpressionContext поверх вложенных словарей.
/// TryGet("document.amount") пройдёт по словарю document, найдёт ключ amount.
/// </summary>
public sealed class DictionaryExpressionContext : IExpressionContext
{
    private readonly IReadOnlyDictionary<string, object?> _root;

    public DictionaryExpressionContext(IReadOnlyDictionary<string, object?> root)
    {
        _root = root;
    }

    public bool TryGet(string path, out object? value)
    {
        if (string.IsNullOrEmpty(path))
        {
            value = null;
            return false;
        }

        object? current = _root;
        var segments = path.Split('.');
        foreach (var segment in segments)
        {
            if (current is IReadOnlyDictionary<string, object?> dict)
            {
                if (!dict.TryGetValue(segment, out current))
                {
                    value = null;
                    return false;
                }
            }
            else
            {
                value = null;
                return false;
            }
        }

        value = current;
        return true;
    }

    public IReadOnlyDictionary<string, object?> ToDictionary() => _root;
}
