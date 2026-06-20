using System.Globalization;
using System.Text.Json;

namespace Cheetah.FeatureManagement.Filters;

/// <summary>
/// Чтение параметров правила. Значения могут быть как CLR-типами (если правило построено в коде),
/// так и <see cref="JsonElement"/> (если пришли из <c>jsonb</c> / десериализованного запроса).
/// </summary>
internal static class FilterParameters
{
    public static int? GetInt(this IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null) return null;
        return v switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) => r,
            JsonElement { ValueKind: JsonValueKind.Number } je => je.GetInt32(),
            JsonElement { ValueKind: JsonValueKind.String } je when int.TryParse(je.GetString(), out var r) => r,
            _ => null
        };
    }

    public static DateTimeOffset? GetDate(this IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null) return null;
        return v switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt, TimeSpan.Zero),
            string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var r) => r,
            JsonElement { ValueKind: JsonValueKind.String } je when DateTimeOffset.TryParse(je.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var r) => r,
            _ => null
        };
    }

    public static string? GetString(this IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null) return null;
        return v switch
        {
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
            _ => v.ToString()
        };
    }

    public static IReadOnlyCollection<string> GetStringSet(this IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null) return Array.Empty<string>();
        switch (v)
        {
            case IEnumerable<string> ss:
                return ss.ToArray();
            case JsonElement { ValueKind: JsonValueKind.Array } je:
                return je.EnumerateArray()
                    .Select(e => e.ValueKind == JsonValueKind.String ? e.GetString() : e.ToString())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s!)
                    .ToArray();
            case System.Collections.IEnumerable en and not string:
                return en.Cast<object?>().Where(o => o is not null).Select(o => o!.ToString()!).ToArray();
            default:
                return new[] { v.ToString()! };
        }
    }
}
