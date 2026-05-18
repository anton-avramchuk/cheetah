# Cheetah.Permissions.Catalog.Client

HTTP-клиент к [`Cheetah.Permissions.Catalog.Api`](../Cheetah.Permissions.Catalog.Api).
Используется микросервисами для регистрации своих permissions в общем каталоге.

## Подключение

```csharp
[DependsOn(typeof(CrmPermissionsCatalogClientModule))]
public partial class MyServiceModule : CrmModule { }
```

`appsettings.json`:
```json
{
  "Permissions": {
    "CatalogClient": {
      "BaseUrl": "https://permissions-admin.internal/",
      "Timeout": "00:00:05",
      "ContinueOnFailure": true
    }
  }
}
```

`RemoteRegistrySyncService` сделает следующее при старте:
1. Просканит сборки на `[Permission]`-атрибуты.
2. Сгруппирует их по `Module` (имя сборки или explicit).
3. На каждую группу — `POST /api/permissions/catalog/sync` с DTO.

Если каталог недоступен и `ContinueOnFailure=true` — приложение продолжит запуск.
