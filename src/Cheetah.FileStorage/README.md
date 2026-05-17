# Cheetah.FileStorage

Core-абстракции файлового хранилища. Никаких провайдеров здесь нет — только контракты. Конкретные хранилища подключаются отдельными модулями:

- [Cheetah.FileStorage.Local](../Cheetah.FileStorage.Local/README.md) — локальная FS
- [Cheetah.FileStorage.S3](../Cheetah.FileStorage.S3/README.md) — AWS S3 / MinIO / Yandex Object Storage
- [Cheetah.FileStorage.Http](../Cheetah.FileStorage.Http/README.md) — клиент удалённого storage-микросервиса

## Состав

| Тип | Назначение |
|-----|------------|
| `IFileStorage` | Базовый контракт: Save / OpenRead / Delete / Exists / GetMetadata |
| `IFileStorageUrlProvider` | Опциональный: presigned URLs (только провайдеры, которые умеют — S3) |
| `FileMetadata` | Размер, content-type, lastModified, userMetadata |
| `FileStorageException` / `FileStorageNotFoundException` | Унифицированные ошибки |
| `StorageKey.Validate` | Защита от path-traversal: проверка корректности ключа |
| `CrmFileStorageModule` | Core-модуль (только определяет абстракции, ничего не регистрирует) |

## Ключи

Ключ — opaque-строка с иерархией через `/`, например `attachments/2026/05/uuid-name.pdf`. Допустимы буквы, цифры, `-`, `_`, `.`, `/`. Запрещены `..`, `.`, пустые сегменты, ведущие/завершающие `/`. Проверяется `StorageKey.Validate`.

## Streaming

`SaveAsync` и `OpenReadAsync` работают с `Stream` — большие файлы не загружаются целиком в память. Возвращаемый из `OpenReadAsync` поток должен быть закрыт вызывающим.

## Использование

```csharp
public class AttachmentService
{
    private readonly IFileStorage _storage;
    public AttachmentService(IFileStorage storage) => _storage = storage;

    public async ValueTask UploadAsync(Guid entityId, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var key = $"attachments/{entityId}/{Guid.NewGuid()}/{fileName}";
        await _storage.SaveAsync(key, content, contentType, cancellationToken: ct);
        // сохранить key в БД как поле сущности
    }
}
```

## Несколько хранилищ в одном приложении

Если нужны разные хранилища для разных категорий файлов (например, временные экспорты — Local, пользовательские вложения — S3), используйте keyed services:

```csharp
services.AddKeyedSingleton<IFileStorage, LocalFileStorage>("temp");
services.AddKeyedSingleton<IFileStorage, S3FileStorage>("attachments");
```

И инжектите через `[FromKeyedServices("attachments")] IFileStorage storage`.
