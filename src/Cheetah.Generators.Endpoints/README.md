# Cheetah.Generators.Endpoints

Incremental source generator: по декларативным endpoint-классам из [Cheetah.Backend.Endpoints](../Cheetah.Backend.Endpoints/README.md) генерирует Minimal API регистрацию маршрутов (`MapGet/MapPost/...` → маппинг → `IDispatcher` → результат). REST-аналог [Cheetah.Generators.Grpc](../Cheetah.Generators.Grpc/README.md).

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.Endpoints\Cheetah.Generators.Endpoints.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Поддерживаемые базовые типы

Генератор ищет наследников этих абстрактных классов и по их generic-параметрам определяет форму генерации:

| База | Тело сгенерированного маршрута |
|------|--------------------------------|
| `QueryEndpoint<,,,>` | `Map → QueryAsync → Map → Ok` |
| `QueryOrNotFoundEndpoint<,,,>` | то же + `null → NotFound` |
| `QueryCollectionEndpoint<,,,>` | коллекция → `Ok` |
| `QueryGridEndpoint<,,,>` | `GridRequest → QueryAsync → GridResult` |
| `CommandEndpoint<,>` | `Map → SendAsync → NoContent` |
| `CommandWithResultEndpoint<,,,>` | `Map → SendAsync<,> → Map → Ok` |
| `CreateCommandEndpoint<,>` | `Map → SendAsync<,Guid> → CreatedAtRoute` |
| `UpdateCommandEndpoint<,>` / `UpdateCommandWithResultEndpoint<,,,>` | PUT |
| `PatchCommandEndpoint<,>` | PATCH |
| `DeleteCommandEndpoint<,>` | DELETE |

## Поведение

- Endpoint'ы группируются по модулю; на модуль эмитится один регистрационный класс.
- Метаданные (маршрут, теги, авторизация/permissions, Cache-Control, rate-limit) берутся из `EndpointConfiguration` endpoint-класса.
- Маппинг request↔command/query и result↔response выполняется через `IObjectMapper` (Mapster).

## Диагностики

- **`ENDPGEN001`** — ошибка разбора endpoint-класса (некорректные generic-параметры/конфигурация).
