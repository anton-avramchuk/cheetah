using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Logic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Expressions.JsonLogic;

/// <summary>
/// Реализация IExpressionEvaluator поверх JsonLogic (https://jsonlogic.com).
/// Stateless, thread-safe — регистрируется как Singleton.
///
/// Расширенные операторы (<c>now</c>, <c>regex</c>) обрабатываются через
/// <see cref="ExpressionPreprocessor"/> ДО передачи в JsonLogic — это позволяет
/// держать движок «чистым» и не зависеть от деталей реестра правил библиотеки.
/// </summary>
public sealed class JsonLogicExpressionEvaluator : IExpressionEvaluator
{
    private readonly ExpressionOptions _options;
    private readonly ILogger<JsonLogicExpressionEvaluator> _logger;

    public JsonLogicExpressionEvaluator(
        IOptions<ExpressionOptions> options,
        ILogger<JsonLogicExpressionEvaluator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask<ExpressionResult<T>> EvaluateAsync<T>(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default)
    {
        if (expression.Length > _options.MaxExpressionLength)
            return ExpressionResult<T>.Fail($"Expression exceeds MaxExpressionLength={_options.MaxExpressionLength}");

        if (GetNestingDepth(expression) > _options.MaxNestingDepth)
            return ExpressionResult<T>.Fail($"Expression nesting depth exceeds MaxNestingDepth={_options.MaxNestingDepth}");

        // Шаг 1: pre-resolve кастомных операторов (now, regex) — заменяются на литералы.
        string preprocessed;
        try
        {
            preprocessed = ExpressionPreprocessor.RewriteSync(expression, context, _options.RegexTimeout);
        }
        catch (JsonException ex)
        {
            return ExpressionResult<T>.Fail($"Parse error: {ex.Message}");
        }

        // Шаг 2: разбор оставшегося в Rule и выполнение JsonLogic.
        Rule rule;
        try
        {
            rule = JsonSerializer.Deserialize<Rule>(preprocessed)
                ?? throw new ExpressionException("Parsed rule is null");
        }
        catch (Exception ex) when (ex is JsonException or ExpressionException)
        {
            return ExpressionResult<T>.Fail($"Parse error: {ex.Message}");
        }

        var dataNode = JsonNodeFromContext(context);

        try
        {
            // rule.Apply — синхронная CPU-работа; токен её прервать не может, поэтому
            // замеряем ТОЛЬКО время самого вычисления (внутри делегата). Так холодный старт
            // пула потоков и JIT первого вызова не учитываются как превышение лимита —
            // иначе первый вызов на медленном раннере ложно падал бы по таймауту.
            var sw = new Stopwatch();
            var resultNode = await Task.Run(() =>
            {
                sw.Start();
                var node = rule.Apply(dataNode);
                sw.Stop();
                return node;
            }, ct);

            if (sw.Elapsed > _options.MaxEvaluationTime)
            {
                _logger.LogWarning("Expression evaluation took {ElapsedMs}ms (limit {LimitMs}ms)",
                    sw.ElapsedMilliseconds, _options.MaxEvaluationTime.TotalMilliseconds);
                return ExpressionResult<T>.Fail($"Expression evaluation exceeded MaxEvaluationTime={_options.MaxEvaluationTime}");
            }

            return TryConvert<T>(resultNode);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Expression evaluation failed");
            return ExpressionResult<T>.Fail($"Evaluation error: {ex.Message}");
        }
    }

    public async ValueTask<bool> EvaluateBooleanAsync(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default)
    {
        var result = await EvaluateAsync<bool>(expression, context, ct);
        return result is { Success: true, Value: true };
    }

    public ExpressionParseResult Parse(string expression)
    {
        if (expression.Length > _options.MaxExpressionLength)
            return ExpressionParseResult.Fail($"Expression exceeds MaxExpressionLength={_options.MaxExpressionLength}");

        try
        {
            using var doc = JsonDocument.Parse(expression);
            // Проверяем что это валидный JsonLogic — пытаемся десериализовать в Rule.
            _ = JsonSerializer.Deserialize<Rule>(expression);

            var variables = new SortedSet<string>(StringComparer.Ordinal);
            CollectVariables(doc.RootElement, variables);
            return ExpressionParseResult.Ok(variables.ToArray());
        }
        catch (JsonException ex)
        {
            return ExpressionParseResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ExpressionParseResult.Fail(ex.Message);
        }
    }

    private static ExpressionResult<T> TryConvert<T>(JsonNode? node)
    {
        if (node is null)
        {
            // Для reference-типов и nullable допустим null
            if (default(T) is null)
                return ExpressionResult<T>.Ok(default!);
            return ExpressionResult<T>.Fail("Result is null but target type is non-nullable value type");
        }

        try
        {
            // bool/числа JsonLogic возвращает как JsonValue; deserialize<T> справится.
            var value = node.Deserialize<T>();
            return ExpressionResult<T>.Ok(value!);
        }
        catch (Exception ex)
        {
            return ExpressionResult<T>.Fail($"Cannot convert result to {typeof(T).Name}: {ex.Message}");
        }
    }

    private static JsonNode? JsonNodeFromContext(IReadOnlyDictionary<string, object?> context)
    {
        // Конвертируем словарь в JsonObject через JsonSerializer — он умеет вложенные структуры.
        var json = JsonSerializer.Serialize(context);
        return JsonNode.Parse(json);
    }

    private static int GetNestingDepth(string expression)
    {
        var depth = 0;
        var max = 0;
        var inString = false;
        var escape = false;
        foreach (var c in expression)
        {
            if (escape) { escape = false; continue; }
            if (c == '\\') { escape = true; continue; }
            if (c == '"') { inString = !inString; continue; }
            if (inString) continue;
            if (c == '{' || c == '[')
            {
                depth++;
                if (depth > max) max = depth;
            }
            else if (c == '}' || c == ']')
            {
                depth--;
            }
        }
        return max;
    }

    private static void CollectVariables(JsonElement element, SortedSet<string> variables)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Name == "var")
                    {
                        switch (prop.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                variables.Add(prop.Value.GetString()!);
                                break;
                            case JsonValueKind.Array:
                                if (prop.Value.GetArrayLength() > 0)
                                {
                                    var first = prop.Value[0];
                                    if (first.ValueKind == JsonValueKind.String)
                                        variables.Add(first.GetString()!);
                                }
                                break;
                        }
                    }
                    else
                    {
                        CollectVariables(prop.Value, variables);
                    }
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectVariables(item, variables);
                break;
        }
    }
}
