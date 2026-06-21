# Cheetah.Modules.CustomFields

Кастомные поля сущностей **без миграций**: администратор объявляет дополнительные поля для
зарегистрированных типов (`crm.deal`, `crm.customer`…), значения хранятся в `jsonb`, валидируются и
управляются по видимости. Конкретный (`sealed`) модуль — расширяемость здесь **на уровне данных**, а не
наследования типов (см. план [`docs/modules/custom-fields.md`](../../../docs/modules/custom-fields.md)).

## Состав (8 сборок + 3 теста)

| Сборка | Назначение |
|---|---|
| `DomainEvents` | `CustomFieldDefinitionCreated/Changed/Deactivated`, `CustomFieldValuesChanged` |
| `Shared` | `CustomFieldDataType`, `CustomFieldEntityIdType`, константы, конвенции ключей |
| `Contracts` | DTO/Request + `CustomFieldEntityTypeDescriptor`; правила валидации — полиморфный `IValidationRule` |
| `Domain` | `CustomFieldDefinition`, `CustomFieldValueSet`, `CustomFieldEntityType`, спецификации, порт `ICustomFieldDefinitionReader` |
| `Infrastructure` | EF Core (`jsonb` + GIN-индекс), миграция, репозитории, кэш-aside чтение определений, инвалидатор |
| `Application` | CQRS определений/значений/реестра, валидация (`Cheetah.Validation`), видимость (`Cheetah.Expressions.JsonLogic`) |
| `Api` | Minimal API: реестр, админка определений, значения, видимые поля |
| `Client` | HTTP-клиент + регистрация расширяемых типов при старте (`ContinueOnFailure`) |

## Ключевые решения

- **Хранилище значений — один `jsonb`-набор на сущность** (`CustomFieldValueSet`: `(TenantId, EntityType,
  EntityId)` → `{key: value}`), не EAV. GIN-индекс — задел под фильтрацию по значениям.
- **Валидация — `Cheetah.Validation`**: `RequiredRule` выводится из `Required`, остальные правила хранятся
  сериализованным JSON в определении. Кросс-полевые правила видят весь словарь. Невалидные значения →
  `CustomFieldsValidationException` (наследник `ArgumentException` → 400).
- **Видимость поля — JsonLogic** над объединением значений сущности и переданного контекста.
- **Реестр типов — push-upsert при старте** (как Tags), с опциональными предопределёнными **глобальными**
  полями; поля тенантов не затираются.
- **Горячее чтение определений — из кэша** (`ICacheService`), инвалидация по событиям изменения на всех
  инстансах через шину.

## Регистрация типов потребителем

```csharp
services.AddCustomFieldsClient(o => o.BaseUrl = cfg["CustomFields:Url"])
        .RegisterCustomFieldTypes(reg =>
        {
            reg.Add("crm.deal", "Сделка", x => x.IdType = CustomFieldEntityIdType.Guid);
            reg.Add("crm.customer", "Клиент", x =>
            {
                x.IdType = CustomFieldEntityIdType.Guid;
                x.PredefinedFields =
                [
                    new("industry", "Отрасль", CustomFieldDataType.Enum, Options: ["IT", "Retail"])
                ];
            });
        });
```

## Композиция чтения

Модуль не «вклеивает» значения в чужие сущности. Потребитель/BFF получает их батчем
(`POST /api/custom-fields/values/batch-get`) и мёржит со своей DTO на уровне API-композиции (анти-N+1).

## REST

```
POST   /api/custom-fields/registry/sync          — upsert каталога типов (Client при старте)
GET    /api/custom-fields/registry               — список зарегистрированных типов
GET    /api/custom-fields/definitions?entityType=&onlyActive=
POST   /api/custom-fields/definitions            — создать определение
PUT    /api/custom-fields/definitions/{id}       — изменить (DataType неизменяем)
DELETE /api/custom-fields/definitions/{id}       — деактивировать (soft)
GET    /api/custom-fields/values?entityType=&entityId=
PUT    /api/custom-fields/values                 — upsert значений (+валидация)
POST   /api/custom-fields/values/batch-get       — значения для списка сущностей
POST   /api/custom-fields/fields/visible         — видимые поля (JsonLogic)
```

## Follow-up

Фильтрация по значениям (`jsonb @>`/GIN-спецификация); gRPC для `batch-get`/`registry`; разрешение
`Reference`-полей; аудит изменений определений (`Cheetah.Audit`); идемпотентность подписок
(`Cheetah.Core.Inbox`); транзакционный Outbox; резолв тенант-скоупа из контекста.
