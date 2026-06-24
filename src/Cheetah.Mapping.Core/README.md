# Cheetah.Mapping.Core

Провайдеро-независимый контракт маппинга объектов. Только абстракция `IObjectMapper` и атрибуты декларативного маппинга. Реализации — Mapster ([Cheetah.Mapping.Mapster](../Cheetah.Mapping.Mapster/README.md)) и Expressions ([Cheetah.Mapping.Expressions](../Cheetah.Mapping.Expressions/README.md)). Зависит только от `Cheetah.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `IObjectMapper` | `Map<TDest>(source)`, `Map<TSrc,TDest>(...)`, in-place `Map(src, dest)`, нетипизированные перегрузки, `ProjectTo<TDest>(IQueryable)` |
| `[MapFrom(sourceType)]` | Класс-назначение маппится из указанного source-типа (маркер на типе-приёмнике) |
| `[MapProperty(sourcePropertyName)]` | Сопоставление свойства с иным именем источника |
| `[MapIgnore]` | Исключить свойство из маппинга |
| `[GenerateMapper(sourceType, destinationType)]` | Декларация пары source→dest вне типов-участников: на классе-реестре или на уровне сборки (`[assembly: ...]`) |
| `[MapMember(destinationType, destMember, sourceMember)]` | Переименование члена для `[GenerateMapper]`, объявленное в реестре (не на типах-участниках); привязка к маппингу по типу-приёмнику |
| `[MapNested(destinationType, destMember)]` | Член-приёмник собирается как вложенный объект (тот же источник → тип члена); тоже объявляется в реестре |
| `[MapConstant(destinationType, destMember, value)]` | Член-приёмник получает константное значение (источник игнорируется), напр. `TokenType = "Bearer"` |
| `CrmMappingCoreModule` | Core-модуль |

### `[MapFrom]` vs `[GenerateMapper]`

`[MapFrom]` висит **на типе-приёмнике**, и генератор кладёт маппер в его сборку — значит приёмник обязан ссылаться на source-тип. Это нормально, когда приёмнику и так позволено знать источник (например, маппинг внутри одной «верхней» сборки), но **нарушает слои для пары Contracts ↔ Domain**: повесив `[MapFrom(typeof(Domain.X))]` на DTO в Contracts, вы тянете в Contracts ссылку на Domain.

`[GenerateMapper]` решает это: маркер размещается в **выделенной маппинг-сборке** (по образцу `*.Mapster`), которая ссылается на обе стороны. Сгенерированные мапперы оседают там, а Contracts/Domain остаются чистыми. См. пилот `Cheetah.Modules.Identity.Mapping`.

## Использование

API/Application-слой инжектит `IObjectMapper` и не зависит от конкретной реализации:

```csharp
var command = mapper.Map<CreateOrderRequest, CreateOrderCommand>(request);
var vm = mapper.Map<Order, OrderViewModel>(order);
IQueryable<OrderViewModel> projected = mapper.ProjectTo<OrderViewModel>(query); // маппинг на стороне SQL
```

Атрибуты `[MapFrom]`/`[MapProperty]`/`[MapIgnore]` обрабатываются генератором [Cheetah.Mapping.Generators](../Cheetah.Mapping.Generators/README.md) — он валидирует полноту маппинга на этапе компиляции.
