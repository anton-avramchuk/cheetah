# Cheetah.Backend.ReverseProxy

Обратный прокси на базе **YARP**: маршрутизация внешних запросов на внутренние кластеры/сервисы. Подключает агрегацию OpenAPI по кластерам (через [Cheetah.OpenApi](../Cheetah.OpenApi/README.md)). Зависит от `Cheetah.Core`, `CrmAspNetCoreModule`, `OpenApiModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmBackendReverseProxyModule` | Модуль; в `OnApplicationInitialization` вызывает `MapReverseProxy()` |
| `AddYarpFromConfig(key)` | Загрузка маршрутов/кластеров из конфигурации (`ReverseProxy`-секция) + service discovery |
| `AddYarpFromMemory(routes, clusters)` | Программная конфигурация маршрутов/кластеров |
| `IClusterAddressProvider` (из OpenApi) | Перечисление адресов кластеров и их префиксов |
| `ConfiguredClusterAddressProvider` / `InMemoryClusterAddressProvider` | Реализации для config- и in-memory-режимов |

## Подключение

Из конфигурации:

```csharp
services.AddYarpFromConfig(); // секция "ReverseProxy"
```

```json
{
  "ReverseProxy": {
    "Routes": { "vacancy": { "ClusterId": "vacancy", "Match": { "Path": "/vacancy/{**catch-all}" } } },
    "Clusters": { "vacancy": { "Destinations": { "d1": { "Address": "http://vacancy-svc/" } } } }
  }
}
```

Программно:

```csharp
services.AddYarpFromMemory(routes, clusters);
```

`IClusterAddressProvider` извлекает префиксы маршрутов (`/vacancy/{**catch-all}` → `/vacancy/`) — используется агрегатором OpenAPI для сборки общей Swagger-страницы по всем сервисам за прокси.
