# Cheetah.Expressions

Абстракции движка выражений. Сами выражения сериализуемы (хранятся в БД / JSON-схемах),
безопасны (никакого произвольного кода), детерминированы и легко тестируются.

Используется в трёх местах:
- **Cheetah.Validation** — кастомные правила валидации.
- **Cheetah.Core.StateMachine.Dynamic** — guard'ы переходов FSM.
- **Cheetah.Workflow** — условия и действия в rules engine.

## Состав

| Тип | Назначение |
|-----|------------|
| `IExpressionEvaluator` | Точка входа: `EvaluateAsync<T>`, `EvaluateBooleanAsync`, `Parse` |
| `ExpressionResult<T>` | Результат вычисления (Success/Value/Error) |
| `ExpressionParseResult` | Результат разбора (Valid, Error, ReferencedVariables) |
| `ExpressionException` / `ExpressionLimitExceededException` | Исключительные ситуации (внутренние сбои) |
| `IExpressionContext` + `DictionaryExpressionContext` | Помощник для построения контекста с dotted-paths |
| `IReferenceLookup` | Источник для оператора `reference_exists` (проверка id в справочнике) |
| `ExpressionOptions` | Лимиты безопасности (длина, глубина, время выполнения, regex-таймаут) |

## Реализация

Подключите `Cheetah.Expressions.JsonLogic` — движок поверх [JsonLogic](https://jsonlogic.com).

## Дизайн

- **Stateless**: `IExpressionEvaluator` регистрируется как Singleton, реализация thread-safe.
- **Лимиты возвращаются как Fail**, не как исключения — UI должен показать ошибку, а не упасть.
- **Парсинг отделён от выполнения** (`Parse`) — UI-редактор может валидировать синтаксис без data.
- **`reference_exists`** — асинхронный шаг (см. реализацию pre-resolve в JsonLogic-пакете),
  работает одинаково в монолите (локальный лукап) и микросервисах (HTTP-клиент).
