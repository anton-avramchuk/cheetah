# Cheetah.AspNetCore.Blazor.Abstractions

Базовые абстракции конфигурации приложения для Blazor-хоста (BFF): что показывать в шапке (лого и заголовок).
Это «шов» — интерфейс объявлен здесь, а реализация поставляется **хостом**, поэтому UI-сборки
(`Cheetah.AspNetCore.Blazor.Layouts` и др.) не зависят от конкретного приложения.

## Что предоставляет

- **`IApplicationConfigurationProvider`** — `Logo` и `Title` для шапки приложения.
- **`ApplicationLogo`** (discriminated union):
  - `ApplicationLogo.Icon(string CssClass)` — лого как иконка (CSS-класс, например Bootstrap Icons);
  - `ApplicationLogo.Image(string Src, string? Alt = null)` — лого как картинка.
- **`ApplicationTitle(string Name, string? Accent = null)`** — заголовок с опциональной «акцентной» частью.

## Как реализовать в хосте

```csharp
[Export(LifetimeType.Singleton, typeof(IApplicationConfigurationProvider))]
public sealed class AppConfigurationProvider : IApplicationConfigurationProvider
{
    public ApplicationLogo  Logo  => new ApplicationLogo.Icon("bi bi-lightning-charge-fill");
    public ApplicationTitle Title => new("Cheetah", Accent: "CRM");
}
```

`Cheetah.AspNetCore.Blazor.Layouts` (`MainLayout` → `AppLogo`/`AppTitle`) инжектит `IApplicationConfigurationProvider`
и рендерит шапку. Без реализации провайдера layout не сможет отобразить бренд — реализация **обязательна на стороне приложения**.

## Зависимости

- `Cheetah.Core` (`CrmBlazorAbstractionsModule` → `[DependsOn(typeof(CoreModule))]`).
- Без `Microsoft.AspNetCore.App` — это контракты (POCO + интерфейс).
- Модуль `partial` + `Cheetah.Generators.Module` (DI из `[Export]`); собственных сервисов не регистрирует.

## Подключение

```csharp
[DependsOn(typeof(CrmBlazorAbstractionsModule))]
public partial class MyModule : CrmModule { ... }
```
