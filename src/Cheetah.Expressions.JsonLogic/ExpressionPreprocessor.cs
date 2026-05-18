using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Cheetah.Expressions.JsonLogic;

/// <summary>
/// Pre-resolver кастомных операторов, не поддерживаемых нативно JsonLogic-движком:
/// <list type="bullet">
///   <item><c>{"now":[]}</c> → ISO-8601 строка текущего UTC-времени.</item>
///   <item><c>{"regex":[pattern, value]}</c> → bool по результату Regex.IsMatch.
///         Если pattern/value заданы как <c>{"var":"..."}</c> — разворачиваются из контекста.</item>
///   <item><c>{"reference_exists":["Dict", id]}</c> → bool по результату <see cref="IReferenceLookup"/>.
///         Шаг async, выполняется отдельно через <see cref="ResolveReferencesAsync"/>.</item>
/// </list>
///
/// Цель: дать «безопасные» расширения над JsonLogic без вмешательства в его внутренний реестр правил.
/// </summary>
public static class ExpressionPreprocessor
{
    private const string OpNow = "now";
    private const string OpRegex = "regex";
    private const string OpReferenceExists = "reference_exists";
    private const string OpVar = "var";

    private static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Синхронный шаг: разворачивает <c>now</c> и <c>regex</c>. Не делает I/O.
    /// </summary>
    public static string RewriteSync(
        string expression,
        IReadOnlyDictionary<string, object?>? context = null,
        TimeSpan? regexTimeout = null)
    {
        var root = JsonNode.Parse(expression);
        var rewritten = RewriteSyncNode(root, context, regexTimeout ?? DefaultRegexTimeout);
        return rewritten?.ToJsonString() ?? expression;
    }

    /// <summary>
    /// Асинхронный шаг: разворачивает <c>reference_exists</c> через <see cref="IReferenceLookup"/>.
    /// Если lookup == null — все вхождения становятся false.
    /// </summary>
    public static async ValueTask<string> ResolveReferencesAsync(
        string expression,
        IReferenceLookup? lookup,
        IReadOnlyDictionary<string, object?>? context = null,
        CancellationToken ct = default)
    {
        var root = JsonNode.Parse(expression);
        var rewritten = await ResolveReferencesNodeAsync(root, lookup, context, ct);
        return rewritten?.ToJsonString() ?? expression;
    }

    // ----- sync rewrite -----

    private static JsonNode? RewriteSyncNode(
        JsonNode? node,
        IReadOnlyDictionary<string, object?>? context,
        TimeSpan regexTimeout)
    {
        switch (node)
        {
            case JsonObject obj when obj.Count == 1:
                var only = obj.First();
                switch (only.Key)
                {
                    case OpNow:
                        return JsonValue.Create(DateTimeOffset.UtcNow.ToString("O"));

                    case OpRegex:
                        return RewriteRegex(only.Value as JsonArray, context, regexTimeout);
                }
                goto default;

            case JsonObject obj:
                foreach (var key in obj.Select(p => p.Key).ToArray())
                {
                    var rewritten = RewriteSyncNode(obj[key], context, regexTimeout);
                    obj[key] = rewritten?.DeepClone();
                }
                return obj;

            case JsonArray arr:
                for (var i = 0; i < arr.Count; i++)
                {
                    var rewritten = RewriteSyncNode(arr[i], context, regexTimeout);
                    arr[i] = rewritten?.DeepClone();
                }
                return arr;

            default:
                return node;
        }
    }

    private static JsonNode? RewriteRegex(
        JsonArray? args,
        IReadOnlyDictionary<string, object?>? context,
        TimeSpan regexTimeout)
    {
        if (args is null || args.Count < 2)
            return JsonValue.Create(false);

        var pattern = ResolveStringArg(args[0], context);
        var value = ResolveStringArg(args[1], context);

        if (pattern is null || value is null)
            return JsonValue.Create(false);

        try
        {
            var regex = new Regex(pattern, RegexOptions.CultureInvariant, regexTimeout);
            return JsonValue.Create(regex.IsMatch(value));
        }
        catch (ArgumentException)
        {
            return JsonValue.Create(false); // невалидный pattern
        }
        catch (RegexMatchTimeoutException)
        {
            return JsonValue.Create(false);
        }
    }

    private static string? ResolveStringArg(JsonNode? node, IReadOnlyDictionary<string, object?>? context)
    {
        return node switch
        {
            JsonValue v when v.TryGetValue<string>(out var s) => s,
            JsonObject obj when obj.Count == 1 && obj.TryGetPropertyValue(OpVar, out var varNode)
                => ResolveVarToString(varNode, context),
            _ => null
        };
    }

    private static string? ResolveVarToString(JsonNode? varNode, IReadOnlyDictionary<string, object?>? context)
    {
        if (context is null) return null;
        var path = varNode switch
        {
            JsonValue v when v.TryGetValue<string>(out var s) => s,
            JsonArray arr when arr.Count > 0 && arr[0] is JsonValue v && v.TryGetValue<string>(out var s) => s,
            _ => null
        };
        if (path is null) return null;
        return new DictionaryExpressionContext(context).TryGet(path, out var value) ? value?.ToString() : null;
    }

    // ----- async rewrite (reference_exists) -----

    private static async ValueTask<JsonNode?> ResolveReferencesNodeAsync(
        JsonNode? node,
        IReferenceLookup? lookup,
        IReadOnlyDictionary<string, object?>? context,
        CancellationToken ct)
    {
        switch (node)
        {
            case JsonObject obj when obj.Count == 1 && obj.TryGetPropertyValue(OpReferenceExists, out var argsNode):
                var exists = await EvaluateReferenceAsync(argsNode as JsonArray, lookup, context, ct);
                return JsonValue.Create(exists);

            case JsonObject obj:
                foreach (var key in obj.Select(p => p.Key).ToArray())
                {
                    var rewritten = await ResolveReferencesNodeAsync(obj[key], lookup, context, ct);
                    obj[key] = rewritten?.DeepClone();
                }
                return obj;

            case JsonArray arr:
                for (var i = 0; i < arr.Count; i++)
                {
                    var rewritten = await ResolveReferencesNodeAsync(arr[i], lookup, context, ct);
                    arr[i] = rewritten?.DeepClone();
                }
                return arr;

            default:
                return node;
        }
    }

    private static async ValueTask<bool> EvaluateReferenceAsync(
        JsonArray? args,
        IReferenceLookup? lookup,
        IReadOnlyDictionary<string, object?>? context,
        CancellationToken ct)
    {
        if (lookup is null || args is null || args.Count < 2)
            return false;

        var dictName = args[0]?.GetValue<string>();
        if (string.IsNullOrEmpty(dictName))
            return false;

        var idNode = args[1];
        object? id;
        if (idNode is JsonObject varObj && varObj.Count == 1 && varObj.TryGetPropertyValue(OpVar, out var varName))
        {
            var path = varName?.GetValue<string>();
            if (path is null || context is null) return false;
            if (!new DictionaryExpressionContext(context).TryGet(path, out id) || id is null) return false;
        }
        else
        {
            id = idNode switch
            {
                JsonValue v when v.TryGetValue<string>(out var s) => s,
                JsonValue v when v.TryGetValue<Guid>(out var g) => g,
                JsonValue v when v.TryGetValue<int>(out var i) => i,
                JsonValue v when v.TryGetValue<long>(out var l) => l,
                _ => idNode?.ToJsonString().Trim('"')
            };
            if (id is null) return false;
        }

        return await lookup.ExistsAsync(dictName, id, ct);
    }
}
