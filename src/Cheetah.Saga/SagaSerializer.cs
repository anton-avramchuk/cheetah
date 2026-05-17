using System.Collections.Concurrent;
using System.Text.Json;

namespace Cheetah.Saga;

public static class SagaSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

    public static string Serialize(object data, out string dataType)
    {
        var type = data.GetType();
        dataType = type.AssemblyQualifiedName
                   ?? throw new InvalidOperationException($"Type {type.FullName} has no AssemblyQualifiedName");
        return JsonSerializer.Serialize(data, type, JsonOptions);
    }

    public static object Deserialize(string json, string dataType)
    {
        var type = TypeCache.GetOrAdd(dataType, name =>
            Type.GetType(name, throwOnError: true)
            ?? throw new InvalidOperationException($"Cannot resolve type '{name}'"));
        return JsonSerializer.Deserialize(json, type, JsonOptions)
               ?? throw new InvalidOperationException($"Cannot deserialize saga data of type {dataType}");
    }
}
