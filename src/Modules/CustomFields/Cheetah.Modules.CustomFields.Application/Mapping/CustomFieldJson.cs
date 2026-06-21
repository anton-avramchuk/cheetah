using System.Text.Json;

namespace Cheetah.Modules.CustomFields.Application.Mapping;

/// <summary>
/// Хелперы для значений кастомных полей: нормализация входного словаря (JsonElement → нативные CLR-типы,
/// чтобы правила валидации работали корректно), сериализация в jsonb-строку и разбор обратно.
/// </summary>
public static class CustomFieldJson
{
    private static readonly IReadOnlyDictionary<string, object?> Empty = new Dictionary<string, object?>();

    /// <summary>Приводит значения словаря к нативным типам (раскрывает JsonElement).</summary>
    public static Dictionary<string, object?> Normalize(IReadOnlyDictionary<string, object?>? values)
    {
        var result = new Dictionary<string, object?>();
        if (values is null) return result;
        foreach (var (k, v) in values)
            result[k] = NormalizeValue(v);
        return result;
    }

    /// <summary>Сериализует словарь значений в jsonb-строку.</summary>
    public static string Serialize(IReadOnlyDictionary<string, object?> values)
        => JsonSerializer.Serialize(values);

    /// <summary>Разбирает jsonb-строку набора значений в словарь (значения — нативные типы).</summary>
    public static IReadOnlyDictionary<string, object?> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Empty;
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return Empty;

        var result = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            result[prop.Name] = NormalizeValue(prop.Value);
        return result;
    }

    private static object? NormalizeValue(object? value) => value switch
    {
        null => null,
        JsonElement e => FromElement(e),
        _ => value
    };

    private static object? FromElement(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.TryGetInt64(out var l) ? l : e.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.Object => e.EnumerateObject().ToDictionary(p => p.Name, p => FromElement(p.Value)),
        JsonValueKind.Array => e.EnumerateArray().Select(FromElement).ToList(),
        _ => e.ToString()
    };
}
