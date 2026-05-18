# Cheetah.Permissions.Catalog

БД-каталог объявленных permissions. Опциональный модуль над [`Cheetah.Permissions`](../Cheetah.Permissions/README.md):
permissions проверяются через claims в любом случае, но если админу нужен UI
«выбери permission из списка для назначения роли» — нужно где-то хранить список доступных
permissions. Это и есть Catalog.

## Когда подключать

- Монолит — в проекте-bootstrap (там, где собирается приложение).
- Микросервисы — на одном «admin»-сервисе. Остальные сервисы используют `Cheetah.Permissions.Catalog.Client`,
  чтобы отправить туда свои permissions при старте.

## Состав

| Тип | Назначение |
|---|---|
| `PermissionDefinition` | Сущность каталога (`Entity<string>`) |
| `PermissionsDbContext` | Postgres-таблица `permissions.Permissions` |
| `SyncRegistryCommand` | Идемпотентно добавляет/обновляет permissions модуля |
| `ListPermissionsQuery` | Список (опц. фильтр по модулю) — для UI |
| `LocalRegistrySyncService` | Hosted-сервис: на старте сканит сборки и синкает в БД |

## Подключение

```csharp
[DependsOn(typeof(CrmPermissionsCatalogModule))]
public partial class MyBootstrapModule : CrmModule { }
```

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Permissions": "Host=localhost;Database=permissions;Username=postgres;Password=..."
  }
}
```

При старте `LocalRegistrySyncService` пройдёт по всем загруженным сборкам, соберёт
permissions через атрибут `[Permission]` и сохранит в каталог. Идемпотентно.

## API (через Cheetah.Permissions.Catalog.Api)

| Метод | URL | Назначение |
|---|---|---|
| POST | `/api/permissions/catalog/sync` | Принимает `RegistrySyncRequest` от микросервиса |
| GET | `/api/permissions/catalog?module=X` | Список permissions, опц. фильтр |
