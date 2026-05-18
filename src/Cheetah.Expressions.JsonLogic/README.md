# Cheetah.Expressions.JsonLogic

Реализация [`IExpressionEvaluator`](../Cheetah.Expressions/README.md) поверх формата
[JsonLogic](https://jsonlogic.com) (nuget `JsonLogic` от gregsdennis).

## Подключение

```csharp
[DependsOn(typeof(CrmExpressionsJsonLogicModule))]
public partial class MyModule : CrmModule { }
```

Биндит секцию `Expressions` из `appsettings.json`:

```json
{
  "Expressions": {
    "MaxExpressionLength": 4096,
    "MaxNestingDepth": 32,
    "MaxEvaluationTime": "00:00:00.1",
    "RegexTimeout": "00:00:00.05"
  }
}
```

## Поддерживаемые операторы

Все встроенные операторы JsonLogic: `==`, `!=`, `>`, `<`, `>=`, `<=`, `and`, `or`, `!`,
`+`, `-`, `*`, `/`, `%`, `if`, `var`, `in`, `map`, `filter`, `reduce`, `merge`, `min`, `max`,
`cat`, `substr`, и т.д. См. [полный список](https://jsonlogic.com/operations.html).

### Расширенные операторы (реализованы через pre-resolve)

| Оператор | Семантика | Когда вычисляется |
|---|---|---|
| `{"now": []}` | ISO-8601 UTC timestamp | Sync, при каждом `EvaluateAsync` |
| `{"regex": [pattern, value]}` | `Regex.IsMatch` с timeout-защитой от ReDoS | Sync. Аргументы — литералы или `{"var":"..."}` (резолвятся из контекста) |
| `{"reference_exists": ["DictName", id]}` | Проверка существования id в справочнике через `IReferenceLookup` | Async. Вызывается **отдельно** через `ExpressionPreprocessor.ResolveReferencesAsync` |

> Расширенные операторы реализованы **не как JsonLogic-rules**, а как pre-resolve трансформации:
> проходим AST → находим вхождения → заменяем на литералы → передаём «чистый» JsonLogic в evaluator.
> Это держит наш расширенный набор операторов независимым от внутреннего реестра правил
> библиотеки и упрощает тестирование.

## Примеры

```jsonc
// Статус "Draft"
{"==": [{"var": "status"}, "Draft"]}

// Сумма > 100k И клиент VIP
{"and": [
  {">": [{"var": "amount"}, 100000]},
  {"==": [{"var": "category"}, "VIP"]}
]}

// Дедлайн в прошлом
{"<": [{"var": "deadline"}, {"now": []}]}

// Номер телефона по маске
{"regex": ["^\\+7\\d{10}$", {"var": "phone"}]}

// Валидный контрагент
{"reference_exists": ["Counterparties", {"var": "document.counterpartyId"}]}
```

## Использование reference_exists

`reference_exists` требует асинхронного лукапа, поэтому он работает в два шага:

```csharp
// Шаг 1: разрешить асинхронные ссылки (заменяет {"reference_exists":...} на bool-литералы)
var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
    expression, lookup, context, ct);

// Шаг 2: вычислить как обычное выражение
var allowed = await evaluator.EvaluateBooleanAsync(resolved, context, ct);
```

В монолите `IReferenceLookup` — это локальный сервис над репозиторием. В микросервисах —
client-библиотека соответствующего сервиса (`MasterData.Client`, `Customer.Client`, ...).
Если `IReferenceLookup` не зарегистрирован — все `reference_exists` дают `false`.

## Лимиты безопасности

| Опция | Default | Назначение |
|---|---|---|
| `MaxExpressionLength` | 4096 | Защита от гигантских выражений |
| `MaxNestingDepth` | 32 | Защита от глубоко вложенных AST |
| `MaxEvaluationTime` | 100 ms | Прерывание зависших выражений (через CancellationToken) |
| `RegexTimeout` | 50 ms | Защита от ReDoS |

Превышение лимита возвращает `ExpressionResult.Fail` (не исключение) — UI должен показать ошибку.
