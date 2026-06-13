# Cheetah.Mapping.Core

Провайдеро-независимый контракт маппинга объектов. Только абстракция `IObjectMapper` и атрибуты декларативного маппинга. Реализации — Mapster ([Cheetah.Mapping.Mapster](../Cheetah.Mapping.Mapster/README.md)) и Expressions ([Cheetah.Mapping.Expressions](../Cheetah.Mapping.Expressions/README.md)). Зависит только от `Cheetah.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `IObjectMapper` | `Map<TDest>(source)`, `Map<TSrc,TDest>(...)`, in-place `Map(src, dest)`, нетипизированные перегрузки, `ProjectTo<TDest>(IQueryable)` |
| `[MapFrom(sourceType)]` | Класс-назначение маппится из указанного source-типа |
| `[MapProperty(sourcePropertyName)]` | Сопоставление свойства с иным именем источника |
| `[MapIgnore]` | Исключить свойство из маппинга |
| `CrmMappingCoreModule` | Core-модуль |

## Использование

API/Application-слой инжектит `IObjectMapper` и не зависит от конкретной реализации:

```csharp
var command = mapper.Map<CreateOrderRequest, CreateOrderCommand>(request);
var vm = mapper.Map<Order, OrderViewModel>(order);
IQueryable<OrderViewModel> projected = mapper.ProjectTo<OrderViewModel>(query); // маппинг на стороне SQL
```

Атрибуты `[MapFrom]`/`[MapProperty]`/`[MapIgnore]` обрабатываются генератором [Cheetah.Mapping.Generators](../Cheetah.Mapping.Generators/README.md) — он валидирует полноту маппинга на этапе компиляции.
