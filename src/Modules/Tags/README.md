# Cheetah.Modules.Tags

Централизованный модуль тэгов (заготовка под микросервис). Хранит словарь тэгов и привязки
«тэг ↔ сущность» для произвольных сущностей других сервисов. Источник истины по тэгам.

Полное проектное описание: [`docs/modules/tags.md`](../../../docs/modules/tags.md).

## Сборки

| Сборка | Назначение |
|---|---|
| `Cheetah.Modules.Tags.DomainEvents` | События `TagCreated/Assigned/Unassigned` |
| `Cheetah.Modules.Tags.Shared` | Enum `TagEntityIdType`, константы, конвенции ключей |
| `Cheetah.Modules.Tags.Contracts` | DTO/Request (`TaggableEntityTypeDto`, `RegistrySyncRequest`, `TagDto`, …); сущность адресуется парой `EntityType` (строка) + `EntityId` (Guid) |
| `Cheetah.Modules.Tags.Domain` | Сущности (`TaggableEntityType`, `Tag`, `TagAssignment`), спецификации |
| `Cheetah.Modules.Tags.Infrastructure` | EF Core `TagsDbContext`, конфигурации, репозитории (вся инфраструктура модуля) |
| `Cheetah.Modules.Tags.Application` | CQRS: реестр, тэги, привязки |
| `Cheetah.Modules.Tags.Api` | Minimal API (`/api/tags/**`) |
| `Cheetah.Modules.Tags.Client` | HTTP-клиент + авто-регистрация применимых типов при старте |

## Ключевая идея — регистрация применимых типов

Tags не знает заранее о «контактах» или «сделках». Каждый сервис при старте регистрирует свои
типы сущностей, пригодные для тэгирования, через `Cheetah.Modules.Tags.Client`:

```csharp
// в bootstrap сервиса-потребителя
services.AddTaggableEntityType("crm.contact", "Контакт", o =>
{
    o.MaxTagsPerEntity = 50;
});
services.AddTaggableEntityType("crm.deal", "Сделка", o =>
{
    o.AllowedGroups.Add("stage");
    o.AllowedGroups.Add("priority");
});
```

`appsettings.json`:
```json
{
  "Tags": {
    "Client": {
      "BaseUrl": "https://tags.internal",
      "OwnerService": "crm-service"
    }
  }
}
```

`TagsRegistrationSyncService` (hosted) при старте отправит объявленные типы в `POST /api/tags/registry/sync`.
Идемпотентно (upsert на стороне Tags).

## Хостинг самого сервиса Tags

Подключить `CheetahTagsApiModule` (тянет Application → Domain/Infrastructure). Строка подключения:
```json
{ "ConnectionStrings": { "Tags": "Host=localhost;Database=tags;Username=postgres;Password=..." } }
```

## API

| Метод | URL | Назначение |
|---|---|---|
| POST | `/api/tags/registry/sync` | Регистрация применимых типов сервиса |
| GET | `/api/tags/registry?ownerService=` | Список зарегистрированных типов |
| POST | `/api/tags` | Создать тэг |
| GET | `/api/tags?group=` | Список тэгов |
| POST | `/api/tags/assignments` | Назначить тэги сущности |
| DELETE | `/api/tags/assignments` | Снять тэги |
| GET | `/api/tags/assignments?entityType=&entityId=` | Тэги сущности |

> Мультитенантность в сущностях не заложена (по решению — модуль однотенантный).

## Что ещё не реализовано (из проектного описания)

- batch-получение тэгов для списка сущностей (анти-N+1) и обратный поиск «сущности по тэгам»;
- очистка orphan-привязок по `EntityDeletedIntegrationEvent`;
- gRPC-эндпоинты для горячих путей;
- группы тэгов как сущность (сейчас `Group` — строковое поле тэга);
- миграции EF Core (`dotnet ef migrations add`).
