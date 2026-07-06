# Этап 4. Модуль `Modules/Tenancy` — каталог тенантов

> [← Бэклог](README.md) · Зависимости: [T-1.2](stage-1-current-tenant.md#t-12),
> [T-2.1](stage-2-encryption.md#t-21) (для T-4.5).
>
> Канон модуля из CLAUDE.md. Модуль **конкретный** (как CustomFields): доменной
> вариативности нет. Все его DbContext — `[IgnoreMultiTenancy]` (хостовая БД).
> Все проекты добавить в `Cheetah.slnx` в `/Modules/Tenancy/`.

<a id="t-41"></a>
## T-4.1 Проекты Events / Shared / Contracts — **S**

**Зависит от:** T-1.2.
**Проекты:** `Cheetah.Modules.Tenancy.DomainEvents`, `.Shared`, `.Contracts`
(нейминг — как у CustomFields/FeatureManagement).

**Шаги:**
1. **DomainEvents** — конкретные события, наследующие абстрактные из `Cheetah.Core.Tenants.Events`:
   `TenancyTenantCreatedEvent(Guid TenantId, string Name, string Code)`,
   `TenancyTenantUpdatedEvent`, `TenancyTenantDeactivatedEvent`, `TenancyTenantActivatedEvent`;
   плюс новые: `TenantModuleProvisionedEvent(Guid TenantId, string ModuleName)`,
   `TenantModuleProvisioningFailedEvent(Guid TenantId, string ModuleName, string Error)`,
   `TenantConnectionStringsChangedEvent(Guid TenantId)` (инвалидация кэша,
   [T-5.1](stage-5-connection-resolver.md#t-51)).
   Все эти события — хост-скоуп: `TenantId`-поле `EventBase` остаётся null, идентификатор
   тенанта — полезная нагрузка.
2. **Shared** — `TenancyModuleConstants` (route prefix `/api/tenancy`, имя строки подключения
   `"Tenancy"`), permission-константы (`Tenancy.Manage`, `Tenancy.View`).
3. **Contracts** — `TenantViewModel` (Id, Name, Code, IsActive, Status, CreatedAt),
   `TenantProvisioningViewModel` (ModuleName, Status, Error, AttemptCount, LastAttemptAt),
   `CreateTenantRequest(Name, Code)`, `UpdateTenantRequest(Name)`,
   `SetTenantConnectionStringRequest(ModuleName, ConnectionString)`.

**DoD:** три сборки без лишних зависимостей (Events → только Core.Events+Core.Tenants;
Shared → Core; Contracts → Core+Shared), в солюшне.

<a id="t-42"></a>
## T-4.2 Domain: агрегат `Tenant`, слаг `Code`, provisioning-сущности — **L**

**Зависит от:** T-4.1.
**Проект:** `Cheetah.Modules.Tenancy.Domain` (+ `Domain.Tests`).

**Сущности:**

| Сущность | Детали |
|---|---|
| `Tenant : TenantEntity<TenancyTenantCreatedEvent, ...>` | добавить `Code` (immutable) и `Status: TenantStatus`; фабрика `Tenant.Create(name, code)` → валидация + `AddDomainEvent(Created)`; методы `Rename(name)` (Code не трогает!), `Activate()`, `Deactivate()`, `MarkProvisioningCompleted()` (Provisioning→Active, событие Activated), `SoftDelete()` (Status=Deleted + RemovedAt) |
| `TenantConnectionString : Entity<Guid>` | `TenantId`, `ModuleName`, `EncryptedValue`, `KeyId`, `UpdatedAt`; уникальный индекс (`TenantId`,`ModuleName`); значение приходит уже зашифрованным (шифрует Application через `IStringEncryptionService` — Domain о криптографии не знает) |
| `TenantModuleProvisioning : Entity<Guid>` | `TenantId`, `ModuleName`, `Status: ProvisioningStatus (Pending/Provisioned/Failed)`, `Error?`, `AttemptCount`, `LastAttemptAt?`; методы `MarkProvisioned()`, `MarkFailed(error)` (инкремент AttemptCount), `Reset()` (для re-provision) |

**Валидация `Code`:** `^[a-z][a-z0-9_]{1,28}$` — compiled `[GeneratedRegex]`; проверка в
фабрике, `CrmException` с текстом правила. Это защита от инъекции в `CREATE DATABASE` —
имя БД строится только из Code.

**Спецификации** (raw LINQ в Application запрещён): `TenantByCodeSpecification`,
`ActiveTenantsSpecification`, `TenantsByStatusSpecification`,
`ProvisioningByTenantSpecification`, `PendingOrFailedProvisioningSpecification`,
`ConnectionStringsByTenantSpecification`.

**Интерфейсы репозиториев** (по канону из CLAUDE.md): `ITenantRepository`,
`ITenantConnectionStringRepository`, `ITenantModuleProvisioningRepository`.

**Тесты (Domain.Tests):** валидация слага (валидные/невалидные, граничные длины);
переходы статусов (нельзя Activate из Deleted и т.п.); Rename не меняет Code;
доменные события добавляются; `MarkFailed` копит AttemptCount.

**DoD:** сборка + ≥15 юнит-тестов зелёные.

<a id="t-43"></a>
## T-4.3 Infrastructure: DbContext, миграция, репозитории — **M**

**Зависит от:** T-4.2.
**Проект:** `Cheetah.Modules.Tenancy.Infrastructure`.

**Шаги:**
1. `TenancyDbContext : CrmTenantsDbContext<...>` с `[ConnectionStringName("Tenancy")]`
   и `[IgnoreMultiTenancy]`. DbSet'ы: Tenants, ConnectionStrings, Provisionings.
2. `IEntityTypeConfiguration` для трёх сущностей: таблицы `Tenants`,
   `TenantConnectionStrings`, `TenantModuleProvisionings`; уникальные индексы
   `Tenant.Code`, (`TenantId`,`ModuleName`) ×2; **`builder.Ignore(e => e.DomainEvents)`**;
   `EncryptedValue` — `text`.
3. Модуль: `[DependsOn(CrmTenantsCoreModule, CrmEntityFrameworkModule, CrmEntityFrameworkPostgreSqlModule)]`,
   `AddDbContext` по канону.
4. Миграция `InitialTenancy` (в Infrastructure, как у CustomFields).
5. Реализации репозиториев `[Export(Scoped)]` — по шаблону из CLAUDE.md.
6. Реализации Core-контрактов:
   - `ITenantStore` → `TenancyTenantStore` поверх `ITenantRepository` + `IMemoryCache`
     (TTL 30–60 с; ключи `tenant:{id}` / `tenant:code:{code}`; инвалидация по событиям
     Updated/Activated/Deactivated — подписка в Application, [T-4.5](#t-45));
   - `ITenantConnectionStringService` → генерирует строки всеми зарегистрированными
     `IModuleConnectionStringProvider` (расшифрованные наружу не отдаёт — см. сигнатуру:
     сервис пишет в репозиторий сам, шифруя через `IStringEncryptionService`);
   - `ITenantMigrationService` → `GetAllActiveTenantsAsync` из стора,
     `GetConnectionStringAsync` = прочитать `TenantConnectionString` + `Decrypt`.

**DoD:** сборка, миграция генерируется и применяется на чистой БД, реализации Core-контрактов
зарегистрированы (перекрывают `NullTenantStore`).

<a id="t-44"></a>
## T-4.4 Исправление `DefaultModuleConnectionStringProvider` — **S**

**Зависит от:** T-4.2 (появился Code).
**Файлы:** `src/Cheetah.Core.Tenants/Services/DefaultModuleConnectionStringProvider.cs`,
`IModuleConnectionStringProvider.cs`.

**Проблема.** Имя БД строится из изменяемого `tenantName` без валидации; комментарий
обещает `tenant_{tenantId}_...`, код делает другое.

**Шаги:**
1. Изменить сигнатуру: `GenerateConnectionString(Guid tenantId, string tenantCode, string baseConnectionString)`
   — передаём **Code**, не Name. Обновить XML-doc и README.
2. Имя БД: `{prefix}_{tenantCode}_{module}` (prefix из `MultitenancyOptions.DatabaseNamePrefix`,
   default `t` → `t_acme_deals`). Defence-in-depth: провайдер повторно валидирует slug-regex
   (даже если Domain уже проверил) и `ModuleName.ToLowerInvariant()` по `^[a-z0-9_]+$`.
3. Юнит-тесты: генерация имени; невалидный code → исключение; исходная строка не мутируется
   (прочие параметры сохраняются).

**DoD:** сигнатура и реализация согласованы, тесты зелёные.

<a id="t-45"></a>
## T-4.5 Application: команды, запросы, оркестрация создания — **L**

**Зависит от:** T-4.3, T-2.1.
**Проект:** `Cheetah.Modules.Tenancy.Application` (+ `Application.Tests`).

**Команды** (все — `ICommandHandler`, `ValueTask`, через репозитории и спецификации):

| Команда | Логика |
|---|---|
| `CreateTenantCommand(Name, Code) : ICommand<Guid>` | 1) уникальность Code (спецификация); 2) `Tenant.Create` (Status=Provisioning); 3) `ITenantConnectionStringService`: по каждому `IModuleConnectionStringProvider` сгенерировать строку → `Encrypt` → `TenantConnectionString`; 4) `TenantModuleProvisioning(Pending)` на каждый модуль; 5) всё в одной транзакции + доменные события **через Outbox** (`OutboxEventBus`) — Redis pub/sub at-most-once, потеря `TenantCreatedEvent` недопустима |
| `UpdateTenantCommand(Id, Name)` | Rename; Code менять нельзя (валидация на уровне контракта — поля просто нет) |
| `ActivateTenantCommand / DeactivateTenantCommand` | переходы статуса + события |
| `SoftDeleteTenantCommand(Id)` | Status=Deleted; БД НЕ трогаем |
| `ReprovisionTenantCommand(Id, ModuleName?)` | `Reset()` записей провижининга (всех или одного модуля) + повторная публикация `TenancyTenantCreatedEvent`-эквивалента (см. [T-6.2](stage-6-provisioning.md#t-62) — консюмер идемпотентен) |
| `SetTenantConnectionStringCommand(Id, ModuleName, ConnectionString)` | «bring your own database»: зашифровать, upsert, опубликовать `TenantConnectionStringsChangedEvent` |
| `RotateEncryptionKeyCommand` | пройти все `TenantConnectionString`, у которых `KeyId != ActiveKeyId`: Decrypt → Encrypt активным → сохранить батчами |
| `MarkModuleProvisionedCommand / MarkModuleProvisioningFailedCommand` (internal) | вызываются консюмером провижининга ([T-6.2](stage-6-provisioning.md#t-62)) и хендлерами `TenantModuleProvisioned/FailedEvent` (микросервисный режим); при последнем `Provisioned` → `tenant.MarkProvisioningCompleted()` → `TenantActivatedEvent` |

**Запросы:** `GetTenantByIdQuery`, `GetTenantByCodeQuery`, `ListTenantsQuery`
(grid через `IGridRepository`, как Catalog), `GetTenantProvisioningStatusQuery`
(join провижининг-записей → `TenantProvisioningViewModel[]`).

**EventHandlers:** подписки на собственные события Updated/Activated/Deactivated →
инвалидация кэша `TenancyTenantStore`.

**Тесты (Application.Tests, стабы репозиториев как в соседних модулях):**
Create — полный граф (тенант+строки+провижининг+outbox-события); дубликат Code → ошибка;
Reprovision сбрасывает только Failed/указанный модуль; последний Provisioned → Active +
событие; ротация перешифровывает только старые KeyId.

**DoD:** сборка + ≥15 тестов.

<a id="t-46"></a>
## T-4.6 Api: эндпоинты + permissions — **M**

**Зависит от:** T-4.5.
**Проект:** `Cheetah.Modules.Tenancy.Api`.

**Шаги:**
1. Minimal API (или декларативные эндпоинты Backend.Endpoints — как Catalog/Identity,
   предпочесть декларативные, это текущий стандарт):
   `POST /api/tenancy/tenants`, `GET /api/tenancy/tenants` (grid),
   `GET /api/tenancy/tenants/{id}`, `PUT /api/tenancy/tenants/{id}`,
   `POST .../{id}/activate|deactivate|reprovision`, `DELETE .../{id}` (soft),
   `GET .../{id}/provisioning`, `PUT .../{id}/connection-strings/{module}`.
2. Все эндпоинты — под permission `Tenancy.Manage` (чтение — `Tenancy.View`);
   константы из Shared, регистрация permissions по образцу других модулей.
   **Расшифрованные строки подключения API никогда не возвращает** — только факт наличия
   и ModuleName.
3. Маппинг Request↔Command через `IObjectMapper`/`[GenerateMapper]` (текущий стандарт —
   generator mapping).

**DoD:** сборка, эндпоинты в OpenAPI, permissions работают.
