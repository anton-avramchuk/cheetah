using System.Text.Json;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Workflow.Application;

/// <summary>Рендерит JSON-шаблон параметров действия, подставляя значения из payload триггера.</summary>
public interface IParameterRenderer
{
    IReadOnlyDictionary<string, object?> Render(string parametersTemplateJson,
        IReadOnlyDictionary<string, object?> payload);
}

/// <summary>
/// Простой рендерер: значения-строки вида <c>"{{trigger.Field}}"</c> заменяются на <c>payload["Field"]</c>;
/// прочие значения проходят как есть. Неизвестная переменная → <c>null</c>. Чистая функция (юнит-тестируема).
/// </summary>
[Export(LifetimeType.Singleton, typeof(IParameterRenderer))]
public sealed class ParameterRenderer : IParameterRenderer
{
    private const string Prefix = "{{trigger.";
    private const string Suffix = "}}";

    public IReadOnlyDictionary<string, object?> Render(string parametersTemplateJson,
        IReadOnlyDictionary<string, object?> payload)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(parametersTemplateJson))
            return result;

        using var doc = JsonDocument.Parse(parametersTemplateJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return result;

        foreach (var prop in doc.RootElement.EnumerateObject())
            result[prop.Name] = RenderValue(prop.Value, payload);

        return result;
    }

    private static object? RenderValue(JsonElement element, IReadOnlyDictionary<string, object?> payload)
        => element.ValueKind switch
        {
            JsonValueKind.String => Substitute(element.GetString()!, payload),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };

    private static object? Substitute(string template, IReadOnlyDictionary<string, object?> payload)
    {
        if (!template.StartsWith(Prefix, StringComparison.Ordinal) ||
            !template.EndsWith(Suffix, StringComparison.Ordinal))
            return template; // обычный литерал

        var key = template[Prefix.Length..^Suffix.Length].Trim();
        return payload.TryGetValue(key, out var value) ? value : null;
    }
}
