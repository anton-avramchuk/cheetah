# Cheetah.Mapping.Generators

Incremental source generator для декларативного маппинга из [Cheetah.Mapping.Core](../Cheetah.Mapping.Core/README.md). По классам с `[MapFrom]` генерирует код маппинга и **на этапе компиляции проверяет, что замаплено каждое свойство** — забытое поле становится ошибкой сборки, а не тихим `null`.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Mapping.Generators\Cheetah.Mapping.Generators.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Что генерирует

Для класса-назначения с `[MapFrom(typeof(Source))]` создаётся маппинг, где свойства сопоставляются с источником по имени; `[MapProperty("Other")]` переопределяет имя источника, `[MapIgnore]` исключает свойство.

```csharp
[MapFrom(typeof(Order))]
public partial class OrderViewModel
{
    public Guid Id { get; init; }

    [MapProperty(nameof(Order.Number))]
    public string Code { get; init; }

    [MapIgnore]
    public string? ComputedLater { get; init; }
}
```

## Диагностики

| Код | Severity | Когда |
|-----|----------|-------|
| `CHMAP01` | Error | Свойство назначения не сопоставлено ни с одним свойством источника (решается `[MapProperty]` или `[MapIgnore]`) |
| `CHMAP02` | Error | `[MapFrom]` не указывает валидный source-тип |

Строгая валидация — главное отличие от рантайм-мапперов: расхождение моделей ловится при компиляции.
