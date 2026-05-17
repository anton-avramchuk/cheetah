# Cheetah.FileStorage.Local

Локальное файловое хранилище для [Cheetah.FileStorage](../Cheetah.FileStorage/README.md).

## Когда использовать

- Dev/test окружения.
- Однонодовые приложения без horizontal scaling.
- Временные файлы внутри одной реплики.

## Подключение

```csharp
[DependsOn(typeof(CrmFileStorageModule))]
[DependsOn(typeof(CrmFileStorageLocalModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"FileStorage": {
  "Local": {
    "RootPath": "/var/lib/myapp/files",
    "StoreContentTypeSidecar": true
  }
}
```

## Безопасность

`LocalFileStorage` дважды защищён от path-traversal:
1. `StorageKey.Validate` отклоняет `..`, `.`, пустые сегменты и спецсимволы.
2. После `Path.Combine` проверяется, что resolved path находится внутри `RootPath`.

## Sidecar-метаданные

При `StoreContentTypeSidecar=true` рядом с каждым файлом создаётся `<name>.meta.json` с `ContentType` и `UserMetadata`. Это позволяет вернуть точные метаданные через `GetMetadataAsync`. Если выключено — `ContentType` вернётся как `"application/octet-stream"`.
