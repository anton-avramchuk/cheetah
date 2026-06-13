# Cheetah.Core

Фундамент всего фреймворка. Не зависит ни от чего внутри Cheetah — все остальные модули прямо или транзитивно зависят от него.

Содержит три кита платформы: **модульность**, **DI-атрибуты** и **базовую инфраструктуру** (исключения, конфигурация, перехватчики).

## Состав

| Область | Ключевые типы | Назначение |
|---------|---------------|------------|
| Modularity | `CrmModule`, `ICrmModule`, `[DependsOn]`, `ModuleManager`, `ModuleLoader` | Система модулей: декларативные зависимости, топологическая сортировка, жизненный цикл |
| Lifecycle | `ServiceConfigurationContext`, `ApplicationInitializationContext`, `IPreConfigureServices`, `IPostConfigureServices`, `IOnPreApplicationInitialization`, `IOnPostApplicationInitialization` | Хуки фаз запуска приложения |
| DI | `[Export(LifetimeType, ...)]`, `LifetimeType`, `IObjectAccessor`, `ObjectAccessor` | Декларативная регистрация сервисов (подхватывается Source Generator'ом) |
| Application | `CrmApplicationBase`, `CrmApplicationFactory`, `CrmApplicationWithInternalServiceProvider` | Bootstrap приложения и построение графа модулей |
| Exceptions | `CrmException`, `IHasErrorCode`, `IHasHttpStatusCode`, `IHasErrorDetails`, `IExceptionNotifier` | Доменные исключения с кодами/HTTP-статусами |
| Interception | `ICrmInterceptor`, `CrmInterceptor`, `ICrmMethodInvocation` | Перехват вызовов методов (cross-cutting) |
| Configuration | `ConfigurationHelper`, `ConfigurationBuilderOptions` | Хелперы конфигурации |

## Жизненный цикл модуля

```
PreConfigureServices → ConfigureServices → PostConfigureServices
  → OnPreApplicationInitialization → OnApplicationInitialization → OnPostApplicationInitialization
```

```csharp
[DependsOn(typeof(SomeOtherModule))]
public partial class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // авто-сгенерированный метод
    }
}
```

Зависимости объявляются через `[DependsOn(typeof(...))]`; `ModuleManager` строит граф и сортирует его топологически, гарантируя порядок инициализации.

## Регистрация сервисов

```csharp
[Export(LifetimeType.Scoped, typeof(IMyService))]
public class MyService : IMyService { }
```

Атрибут `[Export]` подхватывается Source Generator'ом, который генерирует `RegisterServices(...)` для модуля. Lifetimes: `Singleton`, `Scoped`, `Transient`.

## Исключения

```csharp
public class OrderNotFoundException : CrmException, IHasHttpStatusCode, IHasErrorCode
{
    public int HttpStatusCode => 404;
    public string Code => "order.not_found";
}
```

Реализация этих интерфейсов позволяет инфраструктуре API автоматически маппить исключение на корректный HTTP-ответ.
