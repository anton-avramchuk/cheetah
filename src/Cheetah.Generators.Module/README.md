# Cheetah.Generators.Module

Incremental source generator: для класса-модуля (`CrmModule`/`ICrmModule`) генерирует метод `RegisterServices(IServiceCollection)`, регистрирующий все сервисы, помеченные `[Export]`. Это «авторегистрация DI», на которую опирается весь фреймворк.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.Module\Cheetah.Generators.Module.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Что генерирует

По одному `partial`-классу-модулю в сборке создаётся `public void RegisterServices(IServiceCollection services)` с регистрацией каждого `[Export(LifetimeType, ...)]`-сервиса с нужным lifetime и service-типами.

```csharp
[Export(LifetimeType.Scoped, typeof(IMyService))]
public class MyService : IMyService { }

// → в RegisterServices:
//   services.AddScoped<IMyService, MyService>();
```

Вызывается из `ConfigureServices`:

```csharp
public partial class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
        => RegisterServices(context.Services);
}
```

## Inbox-идемпотентность

Обработчики событий (`IEventHandler<>`), помеченные `[Idempotent]` (из `Cheetah.Core.Inbox`), регистрируются обёрнутыми в декоратор `InboxIdempotentEventHandler` поверх `IInboxStore` — дедупликация по `EventId` происходит прозрачно.

## Требования и диагностики

- Класс модуля должен быть `partial` (иначе нельзя дописать метод).
- Ровно один класс, реализующий `IModule`, на сборку.
- **`MODGEN001`** — модуль не найден / найдено несколько / класс не `partial`.
