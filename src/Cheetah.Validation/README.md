# Cheetah.Validation

Декларативные правила валидации с поддержкой кросс-полевых условий через [Cheetah.Expressions](../Cheetah.Expressions/README.md).

В отличие от FluentValidation — правила это **данные** (сериализуемы в JSON), поэтому
их можно хранить в схеме типа документа и редактировать в UI.

## Состав

| Тип | Назначение |
|-----|------------|
| `IValidationRule` | Контракт правила. `[JsonPolymorphic]` позволяет хранить как JSON |
| `IValidationEngine` / `ValidationEngine` | Применяет наборы правил, агрегирует ошибки |
| `IValidationRuleSerializer` / `ValidationRuleSerializer` | Json roundtrip |
| `ValidationResult` / `ValidationError` | Результат (список ошибок, не исключение) |
| `ValidationContext` | Контекст применения правила (поле + все значения + DI) |
| `FieldValidation` | "Описание валидации одного поля": путь, значение, набор правил |

## Встроенные правила

| Правило | Параметры | Семантика |
|---|---|---|
| `RequiredRule` | — | Не null и (для строк) не whitespace |
| `StringLengthRule` | `Min`, `Max` | Длина строки в диапазоне |
| `NumberRangeRule` | `Min`, `Max` (decimal) | Число в диапазоне; принимает int/long/decimal/double/string |
| `DateRangeRule` | `Min`, `Max` (DateTimeOffset) | Дата в диапазоне; принимает DateTime/DateTimeOffset/ISO-строку |
| `PatternRule` | `Pattern` (regex) | Match по regex; ReDoS-защита (50 ms timeout) |
| `EnumValueRule` | `AllowedValues` | Значение из списка (по `ToString()`) |
| `CompareRule` | `OtherFieldPath`, `Operator` | Сравнение с другим полем (Eq/Ne/Gt/Ge/Lt/Le) |
| `RequiredIfRule` | `Expression` (JsonLogic) | Required, если выражение → true |
| `ExpressionRule` | `Expression`, `Message?` | Произвольное JsonLogic-условие |
| `UniqueRule` | `Scope` | **Placeholder.** Реализация в `Cheetah.Documents` |

## Подключение

```csharp
[DependsOn(typeof(CrmValidationModule))]
public partial class MyModule : CrmModule { }
```

Для `ExpressionRule` / `RequiredIfRule` дополнительно подключите
[`CrmExpressionsJsonLogicModule`](../Cheetah.Expressions.JsonLogic/README.md).

## Использование

```csharp
var fields = new[]
{
    new FieldValidation("name", input.Name,
        new IValidationRule[] { new RequiredRule(), new StringLengthRule { Min = 3, Max = 100 } }),

    new FieldValidation("amount", input.Amount,
        new IValidationRule[] { new NumberRangeRule { Min = 0, Max = 1_000_000 } }),

    new FieldValidation("endDate", input.EndDate,
        new IValidationRule[] { new CompareRule { OtherFieldPath = "startDate", Operator = CompareOperator.Gt } }),
};

var allValues = new Dictionary<string, object?>
{
    ["name"] = input.Name,
    ["amount"] = input.Amount,
    ["startDate"] = input.StartDate,
    ["endDate"] = input.EndDate
};

var result = await engine.ValidateAsync(fields, allValues, serviceProvider, ct);
if (!result.IsValid)
{
    foreach (var err in result.Errors)
        logger.LogWarning("{Field}: {Code} — {Message}", err.FieldPath, err.Code, err.Message);
}
```

## Сериализация набора правил

```csharp
var rules = new IValidationRule[]
{
    new RequiredRule(),
    new StringLengthRule { Min = 3, Max = 100 }
};
var json = serializer.SerializeMany(rules);
// [{"$type":"RequiredRule"},{"$type":"StringLengthRule","min":3,"max":100}]

var back = serializer.DeserializeMany(json);
```

## Дизайн

- **Список ошибок, не исключения** — UI должен подсвечивать все поля сразу.
- **Кросс-полевые правила** видят `AllValues` через контекст.
- **DI доступен правилам** через `ValidationContext.Services` — для правил, требующих I/O
  (например, `ExpressionRule` достаёт `IExpressionEvaluator`).
- **JSON-полиморфизм** через `$type` discriminator — стабильное имя класса. Расширение
  через `[JsonDerivedType]` на интерфейсе.
