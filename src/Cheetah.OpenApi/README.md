# Cheetah.OpenApi

OpenAPI-документ для приложения с трансформерами под специфику Cheetah: примеры тел запросов, параметры грида и агрегация документов нескольких сервисов за reverse proxy. UI поверх документа даёт [Cheetah.Scalar](../Cheetah.Scalar/README.md). Зависит от `Cheetah.Core`, `CrmAspNetCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `OpenApiModule` | Регистрирует `AddOpenApi` с набором трансформеров; в `OnApplicationInitialization` маппит `/openapi` (anonymous) |
| `IOpenApiAggregator` / `OpenApiAggregator` | Сборка единого документа из нескольких сервисов |
| `IClusterAddressProvider` | Источник адресов/префиксов сервисов (реализуется в [Cheetah.Backend.ReverseProxy](../Cheetah.Backend.ReverseProxy/README.md)) |
| `AddOpenApiAggregator()` | Регистрация агрегатора |

## Трансформеры

- **Schema** — генерирует пример тела (`schema.Example`) даже для позиционных record'ов (через рефлексию, т.к. `JsonTypeInfo.Properties` для них пуст), корректно обрабатывает `$ref`, `anyOf/oneOf` (nullable в OpenAPI 3.1), форматы `uuid`/`date-time`/`email`. `JsonElement` отображается как свободный объект.
- **Operation (grid)** — для GET-endpoint'ов, возвращающих `GridResult<T>`, добавляет query-параметры грида (`page`, `pageSize`, `sort[...]`, `filter[...]`) в Swagger.
- **Document** — подставляет `OpenApi:BasePath` как server URL (актуально за reverse proxy).

## Конфигурация

```json
{
  "OpenApi": {
    "BasePath": "/api/orders",
    "DocumentName": "v1",
    "GridParameters": true,
    "SchemaExamples": true
  }
}
```

- `DocumentName` — имя документа (по умолчанию `v1`); задаётся там, где спека называется иначе.
  Иначе сервис со своим `AddOpenApi("crm", …)` получает **два** документа с одними операциями:
  лишний файл при генерации спеки на сборке и лишняя страница в UI.
- `GridParameters` / `SchemaExamples` — выключаются, когда документ описывает чужой контракт.
  Например, BFF принимает плоские параметры отбора, и восемь grid-параметров в спеке означали бы
  восемь бесполезных аргументов в сгенерированном клиенте SPA.

## Агрегация за proxy

`OpenApiAggregator` обходит адреса из `IClusterAddressProvider`, забирает их OpenAPI-документы и склеивает в один — единая Swagger/Scalar-страница по всем сервисам за [reverse proxy](../Cheetah.Backend.ReverseProxy/README.md).
