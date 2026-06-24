# Cheetah.Mapping.Generators

Incremental source generator для декларативного маппинга из [Cheetah.Mapping.Core](../Cheetah.Mapping.Core/README.md). Генерирует код маппинга и **на этапе компиляции проверяет, что замаплено каждое свойство** — забытое поле становится ошибкой сборки, а не тихим `null`. Поддерживает два режима объявления: `[MapFrom]` (на типе-приёмнике) и `[GenerateMapper]` (в выделенной маппинг-сборке).

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

## Режим B: `[GenerateMapper]` (без загрязнения слоёв)

`[MapFrom]` требует маркер **на типе-приёмнике**, поэтому маппер генерируется в его сборку и приёмник вынужден ссылаться на source-тип. Для пары Contracts ↔ Domain это недопустимо (Contracts получил бы ссылку на Domain). `[GenerateMapper(source, dest)]` объявляет пару **вне типов-участников** — на классе-реестре или на уровне сборки — и генерирует маппер в неймспейс этого объявления (для assembly-level — в неймспейс по имени сборки). Маркер ставится в выделенной маппинг-сборке (как `*.Mapster`), которая и так ссылается на обе стороны; Contracts/Domain остаются чистыми.

```csharp
// в сборке Cheetah.Modules.Identity.Mapping
[GenerateMapper(typeof(CreateRoleRequest), typeof(CreateRoleCommand))]
[GenerateMapper(typeof(RoleModel), typeof(RoleViewModel))]
public static partial class IdentityMappingRegistry { }

// либо на уровне сборки:
[assembly: GenerateMapper(typeof(CreateRoleRequest), typeof(CreateRoleCommand))]
```

→ генерирует `source.MapToCreateRoleCommand()` и `query.ProjectToCreateRoleCommand()` в неймспейсе сборки-реестра.

### Переименования без атрибутов на типах (`[MapMember]`)

В режиме `[MapFrom]` переименование задаётся `[MapProperty]` на свойстве приёмника. В режиме `[GenerateMapper]` приёмник трогать нельзя (он в Application/Contracts), поэтому переименование объявляется **в том же реестре** через `[MapMember(destType, destMember, sourceMember)]` — привязка к маппингу по типу-приёмнику. `[MapMember]` имеет приоритет над поиском по имени и над `[MapProperty]`.

```csharp
// id из роута приходит как Id, команда ждёт DocumentId — переименование в реестре:
[GenerateMapper(typeof(IssueDocumentRequest), typeof(IssueDocumentCommand), GenerateProjection = false)]
[MapMember(typeof(IssueDocumentCommand), nameof(IssueDocumentCommand.DocumentId), nameof(IssueDocumentRequest.Id))]
public static partial class SalesDocumentsMappingRegistry { }
```

→ `new IssueDocumentCommand(source.Id)`.

### Вложенные объекты (`[MapNested]`)

Если член приёмника — это вложенный DTO, который надо собрать из плоских полей источника, используйте `[MapNested(destType, destMember)]`. Тип вложенного объекта берётся из типа члена; тот же источник маппится в него по обычным правилам (рекурсивно).

```csharp
// AddLineCommand(Guid DocumentId, AddLineRequest Line) ← плоский AddLineToDocumentRequest
[GenerateMapper(typeof(AddLineToDocumentRequest), typeof(AddLineCommand), GenerateProjection = false)]
[MapMember(typeof(AddLineCommand), nameof(AddLineCommand.DocumentId), nameof(AddLineToDocumentRequest.Id))]
[MapNested(typeof(AddLineCommand), nameof(AddLineCommand.Line))]
```

→ `new AddLineCommand(source.Id, new AddLineRequest(source.ProductId, source.Qty, …))`.

### Константы (`[MapConstant]`)

Член-приёмник с фиксированным значением (источник игнорируется) задаётся `[MapConstant(destType, destMember, value)]`. Значение — константа, допустимая в атрибуте (string/числовой/bool/enum).

```csharp
// TokenViewModel(AccessToken, TokenType, ExpiresIn) ← TokenResult(Token, ExpiresInSeconds)
[GenerateMapper(typeof(TokenResult), typeof(TokenViewModel), GenerateProjection = false)]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.AccessToken), nameof(TokenResult.Token))]
[MapConstant(typeof(TokenViewModel), nameof(TokenViewModel.TokenType), "Bearer")]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.ExpiresIn), nameof(TokenResult.ExpiresInSeconds))]
```

→ `new TokenViewModel(source.Token, "Bearer", source.ExpiresInSeconds)`.

Приоритет резолва члена: `[MapConstant]` → `[MapNested]` → `[MapMember]` → `[MapProperty]` → совпадение по имени. Чего генератор НЕ умеет — произвольных выражений из источника (вычисления, условия, вызовы методов): такое остаётся за Mapster.

По умолчанию генерируются и скалярный `MapTo*`, и `ProjectTo*` для `IQueryable`. Проекция полезна только для read-моделей (Model → ViewModel над запросом к БД); для Request → Command/Query она бессмысленна — отключите её через `GenerateProjection = false`, тогда сгенерируется только `MapTo*`:

```csharp
[GenerateMapper(typeof(CreateRoleRequest), typeof(CreateRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(RoleModel), typeof(RoleViewModel))] // read-проекция → ProjectTo нужен
```

## Построение приёмника

Оба режима умеют создавать приёмник как через **безпараметрный конструктор + инициализатор** (классы с `{ get; set; }`), так и через **конструктор с параметрами** (`record`-типы, immutable-классы) — выбирается публичный конструктор с наибольшим числом параметров; параметры сопоставляются с источником по имени (с регистронезависимым запасным вариантом), остальные сеттабельные свойства заполняются инициализатором.

Свойства собираются **по всей цепочке наследования** (производные перекрывают базовые по имени): Domain-сущности наследуют `Id` от `Entity<T>`, а grid-запросы — пагинацию (`Page`/`PageSize`/`Sort`/`Filter`) от базового запроса.

## Диагностики

| Код | Severity | Когда |
|-----|----------|-------|
| `CHMAP01` | Error | Свойство/параметр назначения не сопоставлены ни с одним свойством источника (решается `[MapProperty]` или `[MapIgnore]`) |
| `CHMAP02` | Error | `[MapFrom]` не указывает валидный source-тип |
| `CHMAP03` | Error | `[GenerateMapper]` не указывает обе стороны (source и destination) |

Строгая валидация — главное отличие от рантайм-мапперов: расхождение моделей ловится при компиляции.
