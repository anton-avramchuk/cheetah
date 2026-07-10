# Cheetah.AspNetCore.Blazor.Navigation

Модуль навигационного меню для Blazor-хоста (BFF). Предоставляет инфраструктуру для динамического построения меню из независимых contributors разных модулей (паттерн «контрибьюторов»).

## Архитектура

```
IMenuContributor          ← реализуют модули, добавляя свои пункты
       ↓
IMenuContributorProvider  ← агрегирует contributors по MenuId
       ↓
INavigationMenuService    ← строит Menu, вызывая каждый contributor
       ↓
NavMenu.razor             ← рендерит готовое Menu
```

Меню находится по строковому идентификатору. Доступные идентификаторы живут в `Constants`:

```csharp
Constants.MainMenuId  // "main" — основное боковое меню
```

Контроль видимости пунктов по правам — через шов `IMenuAccessEvaluator` (`CanSee(ClaimsPrincipal, requiredPermission)`); реализация поставляется приложением/модулем Identity, сам Navigation от Identity не зависит. Тип claim-а разрешения — `Constants.PermissionClaimType`.

Видимость по фич-флагам — второй шов, `IMenuFeatureEvaluator` (`IsEnabledAsync(feature)`), реализуемый приложением поверх `IFeatureManager`; Navigation так же не зависит от FeatureManagement. По умолчанию зарегистрирован `NullMenuFeatureEvaluator` (все фичи включены), поэтому хост без движка флагов работает как раньше. Пункт прячется через `.RequireFeature("Vacancy.Teams")`.

Оба правила складываются в `MenuVisibility`: пункт виден, если выполнены **и** право, **и** фича; выключенная фича на группе гасит её целиком вместе с детьми. Состояние фич считается один раз при построении меню (`MenuVisibility.CollectDisabledAsync`) — по одному запросу на различный ключ, дальше `CanSee` синхронна.

> Меню — это не защита: скрытый пункт не мешает открыть URL напрямую. Страницу гейтите отдельно.

## Модель меню

```
Menu
 └── MenuSection[]     (секция с заголовком, например "Рекрутинг")
      └── MenuItem[]   (пункт меню)
           └── MenuItem[]  (дочерние пункты, если это группа)
```

`MenuItem.IsGroup == true` когда у пункта есть дочерние элементы — NavMenu рендерит его как раскрывающуюся группу.

## Подсветка активного пункта

`MenuActiveResolver.Resolve(candidateUrls, relativePath)` определяет активный пункт по правилу
**«выигрывает самый специфичный (длинный) совпавший URL»** — вместо префиксного матчинга `NavLink`.
Совпадение идёт по границе сегмента (`/team` не матчит `/teams`), пустой URL — только домашняя.

Это решает типовую проблему: при пунктах `/teams` и `/teams/roles`
- на `/teams/roles` активен **только** `/teams/roles` (а не оба);
- на детали сущности `/teams/{id}` активен `/teams` (а не `/teams/roles`).

`NavMenu` собирает URL всех ссылок меню и подсвечивает тот пункт, чей URL вернул резолвер.

## Как добавить пункты меню из своего модуля

### 1. Реализуй `IMenuContributor`

```csharp
using Cheetah.Core.DependencyInjection;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class MyMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("recruiting", "Рекрутинг", order: 1);

        // Простой пункт со ссылкой
        section.AddItem("interviews", "Собеседования")
            .WithIcon("bi bi-camera-video-fill")
            .WithUrl("interviews")
            .WithOrder(1);

        // Группа с дочерними пунктами
        section.AddItem("pipeline", "Пайплайн")
            .WithIcon("bi bi-kanban-fill")
            .WithOrder(0)
            .AddChild("pipeline-active", "Активные", c => c.WithUrl("pipeline/active").WithOrder(0))
            .AddChild("pipeline-archive", "Архив",   c => c.WithUrl("pipeline/archive").WithOrder(1));

        return Task.CompletedTask;
    }
}
```

### 2. Подключи зависимость модуля

В модуле своей сборки добавь `[DependsOn(typeof(CrmBlazorNavigationModule))]`:

```csharp
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
```

> `[Export]` + `Cheetah.Generators.Module` зарегистрируют contributor автоматически — вручную в DI ничего добавлять не нужно.

## API билдеров

### `MenuBuilder`

| Метод | Описание |
|---|---|
| `AddSection(id, label, order)` | Добавляет секцию, возвращает `MenuSectionBuilder` |

### `MenuSectionBuilder`

| Метод | Описание |
|---|---|
| `AddItem(id, label)` | Добавляет пункт, возвращает `MenuItemBuilder` |

### `MenuItemBuilder`

| Метод | Описание |
|---|---|
| `WithIcon(cssClass)` | Bootstrap Icons класс, например `"bi bi-gear-fill"` |
| `WithUrl(relativeUrl)` | Относительный URL, например `"settings/general"`. Пустая строка — главная страница |
| `WithOrder(int)` | Порядок сортировки внутри секции или группы |
| `AddChild(id, label, configure)` | Добавляет дочерний пункт (пункт становится группой) |

## Порядок отображения

Секции сортируются по `order` параметру `AddSection`. Пункты внутри секции и дочерние пункты — по `WithOrder`. Если `WithOrder` не вызван, порядок равен `0` (пункты отображаются в порядке добавления при равных значениях).

Несколько contributors могут добавлять пункты в **одну и ту же секцию** — достаточно передать одинаковый `id` секции. Финальное меню строится заново при каждом вызове `GetMenuAsync`.

## Зависимости

- `Cheetah.Core` (`CrmBlazorNavigationModule` → `[DependsOn(typeof(CoreModule))]`).
- Без `Microsoft.AspNetCore.App`: это чистый сервисный модуль (рендеринг меню — в `Cheetah.AspNetCore.Blazor.Layouts`/`NavMenu`).
- DI генерируется из `[Export]`; `IMenuContributorProvider`/`INavigationMenuService` — `Singleton`.
- Шов `IMenuAccessEvaluator` реализуется приложением (Identity); без реализации все пункты считаются видимыми (зависит от провайдера).
- Шов `IMenuFeatureEvaluator` реализуется приложением (поверх `IFeatureManager`); дефолт — `NullMenuFeatureEvaluator` через `TryAdd`, все фичи включены.
