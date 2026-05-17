# Cheetah.FileStorage.Http

HTTP-клиент удалённого file-storage сервиса. Реализация [IFileStorage](../Cheetah.FileStorage/README.md), которая делегирует все операции внешнему REST-сервису.

## Когда использовать

- **Микросервисная архитектура**: в инфраструктуре есть один центральный storage-сервис, который сам решает где хранить (Local / S3 / GCS). Все остальные сервисы вызывают его через эту обёртку.
- **Изоляция доступа к S3**: только storage-сервис держит AWS-креды, остальные ходят к нему по внутренней сети с короткоживущими токенами.
- **Антивирус / трансформации**: storage-сервис на лету сканирует, ресайзит, конвертирует.

## REST-контракт удалённого сервиса

| Метод | Endpoint | Назначение |
|------|----------|-----------|
| `PUT` | `{BaseUrl}/files/{key}` | Тело — содержимое, `Content-Type` обязателен. Кастомные метаданные — заголовки `X-Meta-*` |
| `GET` | `{BaseUrl}/files/{key}` | Возвращает содержимое; 404 если нет |
| `DELETE` | `{BaseUrl}/files/{key}` | 204; 404 трактуется как успех (идемпотентность) |
| `HEAD` | `{BaseUrl}/files/{key}` | Существует? |
| `GET` | `{BaseUrl}/files/{key}/metadata` | JSON: `{size,contentType,lastModified,userMetadata}` |

## Подключение

```csharp
[DependsOn(typeof(CrmFileStorageModule))]
[DependsOn(typeof(CrmFileStorageHttpModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"FileStorage": {
  "Http": {
    "BaseUrl": "http://storage-service.internal",
    "ApiKey": "internal-bearer-token",
    "Timeout": "00:02:00"
  }
}
```

## Resilience

Под нагрузкой полезно добавить retry/circuit-breaker через `Microsoft.Extensions.Http.Resilience`:

```csharp
services.AddHttpClient(HttpFileStorage.HttpClientName)
    .AddStandardResilienceHandler();
```

Это даст автоматический retry на 5xx и transient network errors.

## Streaming

`SaveAsync` шлёт `Content-Type` и тело потоком (без буферизации в память). `OpenReadAsync` тоже стримит ответ — `HttpResponseMessage` остаётся живым пока поток не закрыт (внутренняя обёртка `OwningStream` следит за этим).
