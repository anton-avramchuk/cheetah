# Мультитенантность — платформенная подключаемая функциональность

> Статус: проектное описание (черновик).
>
> Модель: **БД-на-тенанта в каждом модуле**. Все модули обязаны работать в двух режимах:
> мультитенантном и безтенантном (single-tenant, как сейчас) — без изменения бизнес-кода.
> Переключение — фактом подключения тенантных модулей к хосту, а не флагом в каждом модуле.

## 0. Инвентаризация: что уже есть в репозитории

| Компонент | Состояние |
|---|---|
| `Cheetah.Core.Tenants` — `TenantEntity<...>` (абстрактный шаблон), абстрактные события `TenantCreated/Updated/Deactivated/Activated`, `IModuleConnectionStringProvider`, `ITenantConnectionStringService`, `ITenantMigrationService` | скелет: интерфейсы без реализаций |
| `Cheetah.Core.EntityFramework.Tenants` — `CrmTenantsDbContext`, `ITenantBasedDbContext`, `TenantDatabaseMigrationManager` (подписчик `TenantCreatedEvent`), `MigrateTenantDatabases()` на старте | скелет; `AddTenantsDbContext` НЕ регистрирует `ITenantBasedDbContext` — менеджер миграций всегда получает пустой список |
| `DbContextOptionsFactory.ResolveConnectionString` | закомментированный код под `ICurrentTenant` — шов подготовлен, самого `ICurrentTenant` нет |
| `IConnectionStringResolver` | **единственная точка резолвинга строк для всех трёх DAL**: EF (`DbContextOptionsFactory`), Dapper (`DbConnectionFactory`), Mongo (`MongoDatabaseProvider`). Есть sync-контракт `Resolve` (требование EF) |
| `EventBase` | `EventId`, `OccurredAt`; `TenantId` нет |
| `ApplicationClaimTypes` | клейма тенанта нет |

Известные дефекты скелета (исправить в рамках работ):

1. `DefaultModuleConnectionStringProvider` строит имя БД из **изменяемого** `tenantName` без
   санитизации (комментарий обещает `tenant_{tenantId}_...`, код подставляет имя). Это и
   инъекция в `CREATE DATABASE`, и поломка при переименовании тенанта. → см. §5 (slug `Code`).
2. `TenantDatabaseMigrationManager` резолвит `ITenantBasedDbContext` из root-провайдера, а не
   из scope.
3. `CrmRedisEventBus.HandleEventAsync` глотает исключения хендлеров (`catch {}` c TODO) —
   упавший провижининг тенанта будет **молча потерян**. Логирование + политика ретраев обязательны.

## 1. Цели

1. Одна БД на тенанта в каждом мультитенантном модуле (PostgreSQL; Mongo-сторы — БД/префикс на тенанта).
2. Создание тенанта ⇒ все мультитенантные модули создают свои БД и накатывают миграции.
3. Строки подключения хранятся централизованно, значение — зашифровано.
4. `TenantId: Guid?` во всех событиях; `null` = безтенантный режим / хост-скоуп.
5. Безтенантный режим: `ICurrentTenant.Id == null` ⇒ поведение в точности как сейчас
   (строки из конфигурации). Ни один модуль не знает, в каком режиме работает.

## 2. Обзор архитектуры

```
HTTP/gRPC запрос
   │
   ▼
TenantResolutionMiddleware ──► ITenantResolveContributor (claim → header → subdomain)
   │                                   │ валидация: тенант существует и активен (кэш)
   ▼                                   ▼
ICurrentTenant (AsyncLocal)  ◄─── ITenantStore (кэш поверх модуля Tenancy)
   │
   ├──► TenantConnectionStringResolver : IConnectionStringResolver
   │        Id == null → DefaultConnectionStringResolver (конфигурация, как сейчас)
   │        Id != null → строка тенанта из кэша (расшифрована)
   │        └── единый шов ⇒ EF + Dapper + Mongo тенантны автоматически
   │
   └──► IEventBus: стемпинг TenantId при Publish, Change(TenantId) при Handle
```

Новые/дорабатываемые сборки:

| Сборка | Содержимое |
|---|---|
| `Cheetah.Core.Tenants` (есть) | + `ICurrentTenant`, `CurrentTenant` (AsyncLocal-accessor), `ITenantStore`, `TenantInfo`, `[IgnoreMultiTenancy]`, `MultitenancyOptions` |
| `Cheetah.Core.Security` (есть) | + `IStringEncryptionService` (AES-256-GCM, версионируемый ключ), + `ApplicationClaimTypes.TenantId` |
| `Cheetah.AspNetCore.Tenants` (новая) | `TenantResolutionMiddleware`, контрибьюторы (claim/header/subdomain), прогрев кэша строк |
| `Cheetah.Core.EntityFramework.Tenants` (есть) | починить регистрацию `ITenantBasedDbContext`, менеджер миграций → работа через провижининг-стейт |
| `Modules/Tenancy` (новый бизнес-модуль) | каталог тенантов: `Tenant`, `TenantConnectionString`, `TenantModuleProvisioning`; реализация `ITenantStore`, `ITenantConnectionStringService`, `ITenantMigrationService`; API управления |

## 3. Ключевые компоненты

### 3.1. `ICurrentTenant` — амбиентный контекст

```csharp
public interface ICurrentTenant
{
    Guid? Id { get; }          // null = хост / безтенантный режим
    string? Code { get; }      // slug для имён БД, логов
    bool IsAvailable { get; }  // Id != null
    IDisposable Change(Guid? id, string? code = null);
}
```

- Реализация на `AsyncLocal<TenantScope>` (по образцу `DbContextCreationContext`).
- `Change` — единственный способ переключить тенанта (middleware, консюмеры шины, фоновые
  джобы, провижининг). Вложенные `Change` восстанавливают предыдущее значение при Dispose.
- Регистрируется **всегда** (в `CrmTenantsCoreModule`): в безтенантном хосте просто никогда
  не заполняется — модулям не нужно ветвиться.

### 3.2. Резолвинг тенанта на входе (`Cheetah.AspNetCore.Tenants`)

Цепочка `ITenantResolveContributor`, порядок = приоритет:

1. **Claim** `tenant_id` из JWT — единственный доверенный источник для аутентифицированных
   запросов. Если клейм есть, header/subdomain **игнорируются** (нельзя дать пользователю
   тенанта A подставить `X-Tenant-Id: B`).
2. **Header** `X-Tenant-Id` — для M2M-вызовов (ServiceAuth-токены тенанта не несут) и
   для dev/тестов. Для user-токенов не учитывается.
3. **Subdomain** (`{code}.crm.example.com`) — для анонимных публичных страниц
   (Booking public pages) и страницы логина.

После резолвинга: проверка через `ITenantStore` (кэш), что тенант существует, `IsActive` и
`Status == Active`; иначе 404 (не 403 — не раскрывать существование тенанта). Тут же —
асинхронный прогрев кэша строк подключения тенанта (см. 3.3, sync-констрейнт).

Identity: при логине выдаём клейм `tenant_id` (+ `tenant_code`); `ApplicationClaimTypes.TenantId = "tenant_id"`.

### 3.3. Резолвинг строк подключения

`TenantConnectionStringResolver : IConnectionStringResolver` — заменяет дефолтный при
подключении тенантности (это и есть «подключаемость»: без него всё работает по конфигурации):

```csharp
public string Resolve(string? name = null)
{
    if (_currentTenant.Id is not { } tenantId)
        return _fallback.Resolve(name);                  // безтенантный режим

    if (IsHostContext(name))                             // [IgnoreMultiTenancy]
        return _fallback.Resolve(name);

    return _cache.Get(tenantId, name)                    // ТОЛЬКО кэш — sync hot path
        ?? throw new CrmException($"Connection string '{name}' for tenant {tenantId} is not provisioned.");
}
```

- **Sync-констрейнт EF**: `Resolve` не имеет права ходить в БД/сеть. Строки тенанта
  загружаются в кэш заранее: (а) middleware прогревает при первом запросе тенанта,
  (б) провижининг кладёт при создании, (в) инвалидация — по событию `TenantConnectionStringsChangedEvent`
  через Redis (у каждого инстанса локальный `IMemoryCache`).
- Расшифровка — один раз при загрузке в кэш; в кэше строка в открытом виде (память процесса),
  TTL + инвалидация по событию.
- `[IgnoreMultiTenancy]` на DbContext/модуле — контекст всегда ходит в хостовую БД
  (сам каталог Tenancy, host-часть FeatureManagement и т.п.).

### 3.4. Модуль `Modules/Tenancy` — каталог тенантов (хостовая БД)

Единственный владелец данных о тенантах. Конкретный модуль (как CustomFields), не абстрактный
шаблон — доменной вариативности здесь нет, а `TenantEntity<...>` из Core остаётся базой.

| Сущность | Поля |
|---|---|
| **Tenant** (AggregateRoot, наследник `TenantEntity`) | `Id`, `Name` (изменяемое, отображаемое), **`Code`** (immutable slug `^[a-z][a-z0-9_]{1,28}$`, уникальный — для имён БД/поддоменов), `IsActive`, `Status` (Provisioning / Active / Deactivated / Deleted), audit |
| **TenantConnectionString** | `TenantId`, `ModuleName`, `EncryptedValue`, `KeyId` (версия ключа шифрования), `UpdatedAt`; PK (`TenantId`,`ModuleName`) |
| **TenantModuleProvisioning** | `TenantId`, `ModuleName`, `Status` (Pending / Provisioned / Failed), `Error?`, `AttemptCount`, `LastAttemptAt` |

API (host-admin, отдельное разрешение в Permissions): CRUD тенантов, статус провижининга,
ручной re-provision, деактивация/активация. Модуль реализует `ITenantStore`,
`ITenantConnectionStringService`, `ITenantMigrationService` из Core.

### 3.5. Шифрование строк подключения

`IStringEncryptionService` в `Cheetah.Core.Security`:

- **AES-256-GCM**, ключ — из конфигурации/переменной окружения/KeyVault
  (`Multitenancy:Encryption:Keys` — словарь `KeyId → base64`), актуальный `KeyId` в опциях.
- Формат значения: `keyId:nonce:ciphertext:tag` (base64). `KeyId` в строке и в колонке ⇒
  ротация ключа = фоновая перешифровка, старый ключ читается до завершения.
- Почему не ASP.NET Data Protection: ключ должен жить вне приложения (микросервисный режим,
  несколько инстансов), нужна явная ротация и отсутствие зависимости Core от ASP.NET.
  DP-реализация может быть альтернативным адаптером.

Дополнительно уменьшаем ценность секрета: в строке тенанта по умолчанию **нет отдельного
пароля** — она собирается из базового шаблона модуля (`ConnectionStrings:{Module}` из
конфигурации) заменой `Database`. Шифруем всё значение целиком на случай кастомных тенантов
(отдельный сервер/креденшалы — enterprise-сценарий «bring your own database»).

### 3.6. Провижининг: создание БД + миграции

Поток (команда `CreateTenantCommand` в Tenancy):

1. Создать `Tenant` со `Status = Provisioning`, сгенерировать строки по всем
   `IModuleConnectionStringProvider`, зашифровать, сохранить; `TenantModuleProvisioning = Pending`
   для каждого модуля. Всё — одна транзакция + **Outbox**.
2. Опубликовать `TenantCreatedEvent` (через Outbox, не напрямую в Redis pub/sub —
   он at-most-once, потеря события = тенант без БД навсегда).
3. Консюмер(ы): в монолите — один `TenantDatabaseMigrationManager` проходит по всем
   `ITenantBasedDbContext`; в микросервисном режиме — **каждый сервис мигрирует только свои
   контексты** (у сервиса и есть только свои). `MigrateAsync` создаёт БД и накатывает
   миграции — идемпотентно, ретраи безопасны.
4. По каждому модулю — отметка `Provisioned/Failed` (в микросервисном режиме — событием
   `TenantModuleProvisionedEvent` обратно в Tenancy).
5. Все модули `Provisioned` ⇒ `Status = Active`, событие `TenantActivatedEvent`.
   Middleware не пускает пользователей в тенанта до `Active`.

**Реконсиляция** (закрывает два сценария: потерянное событие и «в систему добавили новый
модуль — у старых тенантов нет его БД»): на старте приложения и по расписанию — пройти
`тенанты × зарегистрированные модули`, для отсутствующих/Failed записей провижининга —
создать БД и накатить миграции. Существующий `MigrateAllTenantsAsync` эволюционирует в это;
при большом числе тенантов — фоном после старта, с ограниченным параллелизмом, не блокируя
готовность приложения.

### 3.7. События: `TenantId` и восстановление контекста

`EventBase`:

```csharp
public abstract record EventBase : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid? TenantId { get; set; }   // set: стемпится шиной, руками не заполнять
}
```

Два обязательных правила во **всех** реализациях `IEventBus` (Redis, Kafka, InMemory, Outbox):

1. **Publish**: если `TenantId == null` и `ICurrentTenant.IsAvailable` — проставить
   автоматически. Хендлеры и доменный код `TenantId` не трогают.
2. **Handle**: перед вызовом хендлеров — `using currentTenant.Change(@event.TenantId)`.
   Без этого консюмер события пишет в хостовую (или чужую) БД — самый опасный класс багов
   мультитенантности. Общую логику вынести в хелпер, чтобы четыре шины не дублировали.

События самого Tenancy (`TenantCreatedEvent` и пр.) — хост-скоуп, `TenantId = null`
(идентификатор тенанта — полезная нагрузка, а не скоуп).

Каналы Redis/топики Kafka остаются общими для всех тенантов — изоляция по полю, не по каналу.

### 3.8. Фоновые процессы — восстановление тенант-контекста

Всё, что работает вне HTTP-запроса, обязано явно устанавливать тенанта:

- **Outbox/Inbox**: таблицы живут в БД модуля ⇒ в мультитенантном режиме **у каждого тенанта
  свой outbox**. Поллер: получить активных тенантов (`ITenantStore`, кэш) → для каждого
  `Change(tenantId)` → опросить outbox этой БД. Публикуемые события уже несут `TenantId`.
- **Workflow-консюмер, Booking-напоминания, BackgroundTasks, cron**: то же правило — джоб
  либо итерирует тенантов, либо получает `TenantId` в своём сообщении/состоянии.
- **Кэш** (Redis, `Cheetah.Core.Cache`): нормализатор ключей добавляет префикс `t:{tenantId}:`
  при `IsAvailable` (FeatureManagement-реплика, CustomFields-дефиниции, Catalog-цены и т.д.).
- **Client-библиотеки (HTTP) и gRPC**: on-behalf-of поток уже несёт тенанта в JWT
  (`UserTokenForwardingHandler`); M2M (`ServiceAuth`) — добавить `X-Tenant-Id` из
  `ICurrentTenant` в `DelegatingHandler`/интерцептор и принимать его на входе только для
  service-токенов.

## 4. Классификация модулей

| Скоуп | Модули | БД |
|---|---|---|
| Host-only (`[IgnoreMultiTenancy]`) | Tenancy; host-часть FeatureManagement (определения флагов — хост, `TenantOverride` уже есть) | одна |
| Tenant-based | Customer, Deals, Leads, Activities, Catalog, SalesDocuments, NotesTimeline, Booking, Calendar, Workflow, CustomFields, Teams, Tags… | по одной на тенанта |
| Решение отдельно | **Identity** — рекомендация: тенантный (пользователи живут в БД тенанта, изоляция полная; логин требует резолвинга тенанта по subdomain/полю формы) + маленький host-realm для админов платформы. Permissions — тенантный. | |

Признак тенантности модуля = регистрация `IModuleConnectionStringProvider` + маркировка его
DbContext'ов как tenant-based. Модуль без провайдера всегда работает по конфигурации — это и
есть безтенантная совместимость.

## 5. Именование БД и безопасность

- Имя БД: `t_{code}_{module}` (например `t_acme_deals`). `Code` — immutable slug, валидируется
  при создании тенанта, никогда не меняется (rename тенанта меняет только `Name`).
- Никогда не подставлять пользовательский ввод в имя БД без валидации slug'а — `CREATE DATABASE`
  не параметризуется.
- Отдельная PG-роль на окружение (не superuser), право `CREATEDB`; для enterprise-тенантов —
  свои серверы/креденшалы через кастомную строку.

## 6. Производительность (цель 10k+ RPS)

- **Резолвинг строки — только память** (см. 3.3). Никакого I/O в `Resolve`.
- **Кэш `DbContextOptions`**: options зависят от строки ⇒ кэшировать по (`TDbContext`, `tenantId`),
  иначе на каждый запрос — пересборка options + новый ServiceProvider EF (дорого).
  `AddDbContextPool` с per-tenant строками несовместим — не использовать.
- **Пулы соединений**: Npgsql держит пул на уникальную строку ⇒ `тенанты × модули` пулов.
  В шаблоне строки задать скромный `Maximum Pool Size` (например 10) и `Idle Lifetime`;
  при росте числа тенантов — PgBouncer (transaction pooling) перед PG. Задокументировать порог.
- Кэш тенантов/строк — `IMemoryCache` + инвалидация по Redis-событию (не распределённый кэш
  на горячем пути).

## 7. Жизненный цикл тенанта

| Переход | Действия |
|---|---|
| Create | §3.6; до `Active` пользователи не допускаются |
| Deactivate | `TenantDeactivatedEvent`; middleware → 404; фоновые джобы пропускают тенанта; данные сохраняются |
| Activate | обратно |
| Delete | двухфазно: soft-delete (`Status=Deleted`, retention-период, экспорт данных) → физический drop БД **отдельной явной админ-операцией** (никогда автоматически по событию) |
| Смена строки подключения (миграция тенанта на другой сервер) | админ-операция: записать новую строку → событие инвалидации кэша; перенос данных — вне скоупа MVP |

## 8. Явно вне скоупа MVP

- Row-level мультитенантность (общая БД с колонкой TenantId) — архитектура допускает
  (шов тот же `IConnectionStringResolver` + `IDataFilter`), но не строим.
- Автоматический перенос тенанта между серверами, шардирование каталога.
- Биллинг/квоты тенантов (место под `TenantFeature`/лимиты — через FeatureManagement).
- UI администрирования (Angular/Blazor) — после API.

## 9. Порядок реализации

1. **Ядро контекста**: `ICurrentTenant` + accessor в `Cheetah.Core.Tenants`;
   `ApplicationClaimTypes.TenantId`; `IStringEncryptionService` (AES-GCM + тесты).
2. **`EventBase.TenantId`** + стемпинг/`Change` во всех четырёх шинах + тесты.
3. **Модуль Tenancy** (host DB): сущности, провижининг-стейт, реализации `ITenantStore`/
   `ITenantConnectionStringService`/`ITenantMigrationService`, API, Outbox.
4. **`TenantConnectionStringResolver`** + кэш + инвалидация; `[IgnoreMultiTenancy]`;
   раскомментировать и дописать шов в `DbContextOptionsFactory`; кэш `DbContextOptions`.
5. **Провижининг**: починка `Cheetah.Core.EntityFramework.Tenants` (регистрация
   `ITenantBasedDbContext`, scope-баг), реконсиляция на старте, статусы.
6. **`Cheetah.AspNetCore.Tenants`**: middleware + контрибьюторы + прогрев кэша; клейм в Identity-логине.
7. **Фоновые потоки**: тенантный Outbox-поллер, tenant-префикс в кэш-ключах,
   `X-Tenant-Id` в M2M-клиентах.
8. **Пилот**: один бизнес-модуль (Catalog или Deals) переводится в tenant-based;
   интеграционный тест: два тенанта — изоляция данных, событий, кэша; и тот же модуль
   в безтенантном хосте — поведение без изменений.
