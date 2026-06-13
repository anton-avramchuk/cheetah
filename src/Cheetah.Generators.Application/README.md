# Cheetah.Generators.Application

Incremental source generator: по корневому классу с `[Bootstrapper]` строит граф зависимостей всех модулей (через `[DependsOn]`), сортирует его топологически и генерирует код bootstrap-а и инициализации приложения.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.Application\Cheetah.Generators.Application.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Что генерирует

Для класса, помеченного `[Bootstrapper]`:
1. **Bootstrap-код** — точка сборки приложения.
2. **Код инициализации** — модули в топологически отсортированном порядке (по `[DependsOn]`), чтобы зависимости конфигурировались и инициализировались раньше зависимых.

```csharp
[Bootstrapper]
public partial class AppBootstrapper { }
```

## Поведение

- Граф собирается рекурсивно по `[DependsOn(typeof(...))]`.
- При обнаружении **циклической зависимости** генерация прерывается с ошибкой, в сообщении — сам цикл (`A -> B -> A`).
- Ожидается **ровно один** класс с `[Bootstrapper]`; иначе — ошибка генерации.

Топологическая сортировка здесь — тот же контракт, что и в рантайме `ModuleManager` (см. [Cheetah.Core](../Cheetah.Core/README.md)), но вычисленная на этапе компиляции.
