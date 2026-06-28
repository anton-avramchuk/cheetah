# Cheetah.AspNetCore.Blazor.Layouts

Каркас оболочки Blazor-хоста (BFF): главный layout с шапкой и боковым меню, страница «не найдено» и
модалка переподключения Blazor Server. Связывает воедино бренд (`Abstractions`), меню (`Navigation`),
диалоги (`Dialogs`) и тосты (`Toast`).

## Что предоставляет

- **`MainLayout`** (`LayoutComponentBase`) — шапка (лого + заголовок из `IApplicationConfigurationProvider`),
  сворачиваемый сайдбар с `NavMenu`, область контента. Монтирует хосты `DialogHost` и `ToastHost`.
  Слоты: `HeaderLeftExtra`, `HeaderCenter` (по умолчанию — строка поиска).
- **`CenterLayout`** (`LayoutComponentBase`) — минималистичный layout без шапки и сайдбара: рендерит контент
  по центру экрана (по горизонтали и вертикали) в колонке шириной до 440px, фон из `--content-bg`.
  Для страниц входа/регистрации/простых сообщений. Слот `ChildContent` (по умолчанию — `Body`).
  Монтирует хосты `DialogHost` и `ToastHost`.
- **`NavMenu`** — рендер бокового меню: берёт `Menu` через `INavigationMenuService.GetMenuAsync(Constants.MainMenuId)`,
  рисует секции, пункты, раскрывающиеся группы.
- **`AppLogo` / `AppTitle`** — рендер `ApplicationLogo`/`ApplicationTitle` из `Abstractions`.
- **`NotFoundPage`** — страница 404 (используется через `UseStatusCodePagesWithReExecute("/not-found", ...)`).
- **`ReconnectModal`** (+ `ReconnectModal.razor.js`) — UI восстановления соединения Blazor Server.

## Подключение

1) Зависимость модуля:

```csharp
[DependsOn(typeof(CrmBlazorLayoutsModule))]
public partial class AppBootstrapperModule : CrmModule { ... }
```

2) Использование layout-а в роутере/страницах:

```razor
@layout Cheetah.AspNetCore.Blazor.Layouts.MainLayout
```

3) В хосте должна быть зарегистрирована реализация `IApplicationConfigurationProvider`
   (см. `Cheetah.AspNetCore.Blazor.Abstractions`) — иначе шапка не отрисует бренд.

4) Сборки с маршрутизируемыми страницами подключаются в `Program.cs` хоста через
   `AddAdditionalAssemblies(typeof(...).Assembly)` — это требование Blazor, не специфика модуля.

## Зависимости

- `Cheetah.Core`, `Cheetah.AspNetCore.Blazor.Abstractions`, `Cheetah.AspNetCore.Blazor.Navigation`,
  `Cheetah.AspNetCore.Blazor.Dialogs`, `Cheetah.AspNetCore.Blazor.Toast`
  (`CrmBlazorLayoutsModule` → соответствующие `[DependsOn]`).
- `Microsoft.AspNetCore.App` (Razor-компоненты, `Microsoft.AspNetCore.Components.Authorization`).
- Это «верхний» UI-модуль: подключив его, вы транзитивно получаете тосты/диалоги/меню.
  Бренд (`IApplicationConfigurationProvider`) и реализацию шва `IMenuAccessEvaluator` поставляет приложение.
