# Cheetah.AspNetCore.Blazor.Theme

Тема приложения из конфигурации: набор акцентных цветов, который превращается в CSS-переменные и
вставляется в `<head>` Blazor-хоста (BFF). Компоненты (`Cheetah.AspNetCore.Blazor.Controls`/`Grid`/`Layouts`)
используют эти переменные.

## Что предоставляет

- **`ThemeOptions`** — биндится из секции `Theme` в `appsettings.json`:

  | Свойство | По умолчанию | CSS-назначение |
  |---|---|---|
  | `Primary` | `#6366f1` | основной/акцентный |
  | `Success` | `#22c55e` | успех |
  | `Danger`  | `#ef4444` | ошибка/опасность |
  | `Warning` | `#f59e0b` | предупреждение |
  | `Info`    | `#3b82f6` | информация |

- **`IThemeService`** — `string BuildCss()`: собирает блок CSS-переменных из `ThemeOptions`.
- **`ThemeStyleTag`** (Razor-компонент) — рендерит `<style>` с результатом `BuildCss()`.
- **`ColorHelper`** — утилиты работы с цветом (осветление/затемнение и т.п.) для производных оттенков.

## Конфигурация

```json
// appsettings.json
{
  "Theme": {
    "Primary": "#6366f1",
    "Success": "#22c55e",
    "Danger":  "#ef4444",
    "Warning": "#f59e0b",
    "Info":    "#3b82f6"
  }
}
```

`CrmBlazorThemeModule.ConfigureServices` сам делает `AddOptions<ThemeOptions>().BindConfiguration("Theme")`.

## Использование

Разместите `<ThemeStyleTag />` в `<head>` (например, в `App.razor`/`HeadOutlet`) — он подставит CSS-переменные
до рендера остальной разметки:

```razor
<head>
    ...
    <ThemeStyleTag />
</head>
```

## Зависимости

- `Cheetah.Core` (`CrmBlazorThemeModule` → `[DependsOn(typeof(CoreModule))]`).
- `Microsoft.AspNetCore.App` (Razor-компонент `ThemeStyleTag`).
- DI генерируется из `[Export]`; `IThemeService` — `Singleton`.

## Подключение

```csharp
[DependsOn(typeof(CrmBlazorThemeModule))]
public partial class MyModule : CrmModule { ... }
```
