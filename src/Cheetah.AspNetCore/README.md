# Cheetah.AspNetCore

ASP.NET Core хостинг-интеграция для Cheetah: подключает модульную систему к пайплайну ASP.NET,
предоставляет доступ к `IApplicationBuilder`/`IEndpointRouteBuilder` из модулей и точку расширения
для регистрации транспортов.

## Что предоставляет

- **`CrmAspNetCoreModule`**:
  - `ConfigureServices`: `AddAuthorization`, `AddHttpContextAccessor`, object-accessor'ы для
    `IApplicationBuilder` и `IEndpointRouteBuilder`, `ValidationExceptionHandler` + `AddProblemDetails`.
  - `OnApplicationInitialization`: `UseExceptionHandler` → `UseRouting` → вызов всех
    `IModuleTransportRegistrar`.
- **Расширения `ApplicationInitializationContext`** (`GetApplicationBuilder`, `GetRouteBuilder`,
  `GetEnvironment`, `GetConfiguration`, `GetOptions<T>` и т.д.).
- **`ValidationExceptionHandler`** — доменные исключения → HTTP `ProblemDetails`
  (`EntityNotFoundException` → 404, `ArgumentException` → 400).

## `IModuleTransportRegistrar` — точка расширения для транспортов

Интерфейс (`Abstractions/IModuleTransportRegistrar.cs`) позволяет модулям регистрировать эндпоинты
произвольного транспорта в общем `IEndpointRouteBuilder` на старте приложения:

```csharp
[Export(LifetimeType.Singleton, typeof(IModuleTransportRegistrar))]
public sealed class MyGrpcRegistrar : IModuleTransportRegistrar
{
    public void Register(IEndpointRouteBuilder rb, IServiceProvider sp)
        => rb.MapGrpcService<MyService>();
}
```

`CrmAspNetCoreModule.OnApplicationInitialization` резолвит `IEnumerable<IModuleTransportRegistrar>`
и вызывает каждый. Механизм намеренно НЕ использует `OnApplicationInitialization`-override модуля,
чтобы несколько source-генераторов (REST-эндпоинты, gRPC, …) не конфликтовали за единственный override.
REST-эндпоинты по-прежнему регистрируются сгенерированным override'ом; gRPC и другие транспорты —
через этот реестр.

## Зависимости

`CoreModule`, `CrmCoreSecurityModule`, `CrmDomainModule`.

## Хост

Бутстраппер вызывает `Bootstrap.Start(builder.Services)`, затем `app.InitializeApplication()`
(см. `ApplicationBuilderExtensions`), который связывает `IApplicationBuilder`/`IEndpointRouteBuilder`
с object-accessor'ами и запускает жизненный цикл модулей.
