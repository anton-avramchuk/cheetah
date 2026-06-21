using System.Globalization;
using System.Text.Json;

namespace Cheetah.Workflow;

/// <summary>
/// Хелперы безопасного чтения отрендеренных параметров действия. Значения могут быть <see cref="JsonElement"/>
/// (подставлены из payload триггера) или примитивами (литералы шаблона) — эти методы скрывают разницу.
/// </summary>
public static class WorkflowActionContextExtensions
{
    public static string? GetString(this WorkflowActionContext context, string key)
        => context.Parameters.TryGetValue(key, out var value) ? Stringify(value) : null;

    public static Guid? GetGuid(this WorkflowActionContext context, string key)
        => Guid.TryParse(context.GetString(key), out var guid) ? guid : null;

    public static int? GetInt(this WorkflowActionContext context, string key)
        => int.TryParse(context.GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null;

    public static bool? GetBool(this WorkflowActionContext context, string key)
    {
        if (!context.Parameters.TryGetValue(key, out var value) || value is null)
            return null;
        return value switch
        {
            bool b => b,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            _ => bool.TryParse(Stringify(value), out var parsed) ? parsed : null
        };
    }

    private static string? Stringify(object? value) => value switch
    {
        null => null,
        JsonElement je => je.ValueKind == JsonValueKind.Null ? null : je.ToString(),
        _ => value.ToString()
    };
}
