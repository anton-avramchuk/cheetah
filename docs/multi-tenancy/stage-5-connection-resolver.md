# Этап 5. Тенантный резолвинг строк подключения

> [← Бэклог](README.md) · Зависимости: [T-1.2](stage-1-current-tenant.md#t-12),
> [T-1.4](stage-1-current-tenant.md#t-14), [T-4.1](stage-4-tenancy-module.md#t-41).
> Ключевой этап: через `IConnectionStringResolver` тенантными становятся все три DAL (EF/Dapper/Mongo).

<a id="t-51"></a>
## T-5.1 Кэш строк подключения + инвалидация — **M**

**Зависит от:** T-1.2, T-4.1 (событие инвалидации).
**Файлы:** `src/Cheetah.Core.Tenants/ConnectionStrings/` →
`ITenantConnectionStringCache.cs`, `TenantConnectionStringCache.cs`,
`TenantConnectionStringsChangedEventHandler` — в Tenancy.Application (подписка).

**Контракт:**
```csharp
public interface ITenantConnectionStringCache
{
    string? Get(Guid tenantId, string? name);                     // sync, БЕЗ I/O — горячий путь
    ValueTask PrewarmAsync(Guid tenantId, CancellationToken ct);  // загрузить все строки тенанта
    void Invalidate(Guid tenantId);
}
```

**Шаги:**
1. Реализация на `IMemoryCache` (локальный, НЕ распределённый — на горячем пути только память):
   ключ `tcs:{tenantId}:{name}`, sliding TTL из опций (default 30 мин).
   `PrewarmAsync` читает через `ITenantMigrationService.GetConnectionStringAsync`
   (расшифровка происходит там) все строки по зарегистрированным
   `IModuleConnectionStringProvider` одним проходом.
2. Инвалидация между инстансами: хендлер `TenantConnectionStringsChangedEvent` (Redis-шина)
   вызывает `Invalidate(tenantId)`; следующий запрос тенанта прогреет заново.
3. Тесты: Get до прогрева → null; после Prewarm → строка; Invalidate → снова null;
   Prewarm идемпотентен.

**DoD:** кэш + подписка + тесты; в кэше строки в открытом виде — задокументировать
(память процесса, компромисс осознанный).

<a id="t-52"></a>
## T-5.2 `TenantConnectionStringResolver : IConnectionStringResolver` — **M**

**Зависит от:** T-5.1, T-1.4.
**Файлы:** `src/Cheetah.Core.Tenants/ConnectionStrings/TenantConnectionStringResolver.cs`
(+ зависимость `Cheetah.Core.Tenants` → `Cheetah.Core.DataAccess`).

**Логика `Resolve(name)` (sync!):**
1. `ICurrentTenant.Id == null` → делегировать `DefaultConnectionStringResolver` (композиция,
   не наследование — fallback внедряется явно).
2. `name` входит в `MultitenancyOptions.HostConnectionStringNames` → fallback (хостовые
   строки для Dapper/Mongo-сторов, у которых нет типа с атрибутом).
3. Иначе → `cache.Get(tenantId, name)`; промах → `CrmException` с текстом
   «строка не спровижинена / вызовите PrewarmAsync» — НИКАКОГО синхронного I/O.
4. `ResolveAsync`: то же, но при промахе кэша сначала `PrewarmAsync`, потом повтор — этот
   путь используют не-EF потребители, где допустим async.

**Регистрация:** в `CrmTenantsCoreModule` через явный `Replace`-механизм, НО только когда
тенантность реально подключена. Критерий: отдельный модуль-активатор
`CrmTenantsRuntimeModule` (новый класс в той же сборке) — хост, желающий мультитенантность,
добавляет его в граф зависимостей; безтенантный хост его не подключает и живёт на
`DefaultConnectionStringResolver`. Это и есть точка «подключаемости» всей фичи.

**Тесты:** все 4 ветки; плюс интеграционный сценарий двух резолверов (замена/без замены).

**DoD:** резолвер + регистрация + тесты; README `Cheetah.Core.Tenants` — раздел
«как хосту включить мультитенантность» (один `[DependsOn(CrmTenantsRuntimeModule)]`).

<a id="t-53"></a>
## T-5.3 Шов в `DbContextOptionsFactory` + `[IgnoreMultiTenancy]` — **S**

**Зависит от:** T-5.2, T-1.4, T-1.1.
**Файлы:** `src/Cheetah.Core.EntityFramework/DependencyInjection/DbContextOptionsFactory.cs`.

**Шаги:**
1. Реализовать закомментированный блок в `ResolveConnectionString<TDbContext>`:
   ```csharp
   if (IgnoreMultiTenancyAttribute.IsIgnored(typeof(TDbContext)))
   {
       var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();
       using (currentTenant.Change(null))
           return connectionStringResolver.Resolve(connectionStringName);
   }
   ```
   Зависимость `Core.EntityFramework` → `Core.Tenants` допустима (Tenants ниже по стеку);
   если решим не связывать сборки — альтернативный вариант: резолвер сам проверяет
   host-имена ([T-5.2](#t-52) шаг 2), а атрибут транслируется в `HostConnectionStringNames` при
   старте (сканирование зарегистрированных контекстов). Выбрать первый вариант, второй
   зафиксировать как отступление, если появится циклическая зависимость.
2. Тест (unit или в EF-тестах): контекст с атрибутом получает хостовую строку даже в
   тенант-контексте; без атрибута — тенантную.

**DoD:** EF-контексты уважают атрибут; закомментированный код удалён.

<a id="t-54"></a>
## T-5.4 Кэш `DbContextOptions` по (контекст, тенант) — **M**

**Зависит от:** T-5.3.
**Файлы:** `src/Cheetah.Core.EntityFramework/DependencyInjection/DbContextOptionsFactory.cs`
и место регистрации `AddApplicationDbContext` (`Extensions/ServiceCollectionExtensions`).

**Проблема.** Options строятся на каждое создание DbContext; со строкой-на-тенанта нельзя
закэшировать одни options на тип. Пересборка options + внутренний ServiceProvider EF на
каждый запрос — недопустимо при 10k RPS.

**Шаги:**
1. `ConcurrentDictionary<(Type contextType, Guid? tenantId), DbContextOptions>` внутри
   фабрики (или отдельный `IDbContextOptionsCache`). Ключ — тенант из `ICurrentTenant` на
   момент создания. Инвалидация записи тенанта — по `TenantConnectionStringsChangedEvent`
   (подписка рядом с [T-5.1](#t-51)) и по `TenantDeactivatedEvent` (не копить мёртвые записи).
2. Убедиться, что `AddDbContextPool` нигде не используется с тенантными контекстами
   (grep по солюшну; если используется — заменить на обычный `AddDbContext` с комментарием
   почему).
3. Бенчмарк-смоук (не обязателен в CI): создание 10k контекстов с кэшем/без — числа в PR.

**DoD:** повторное создание контекста того же тенанта не пересобирает options; тест на
изоляцию options между тенантами (разные строки).

<a id="t-55"></a>
## T-5.5 Проверка Dapper/Mongo на тенантном резолвере — **S**

**Зависит от:** T-5.2.
**Файлы:** `src/Cheetah.Core.Dapper/Connections/DbConnectionFactory.cs`,
`src/Cheetah.Core.Mongo/Connections/MongoDatabaseProvider.cs` — только чтение/тесты.

**Шаги:**
1. Убедиться, что оба идут через `IConnectionStringResolver` (да) и используют
   `ResolveAsync`, а не sync `Resolve` (у них нет EF-констрейнта — им доступен путь с
   автопрогревом). Если sync — перевести на async-путь.
2. Mongo-сторы (Outbox/Inbox/Audit/Saga) — решить per-store: Outbox/Inbox/Audit —
   тенантные (данные тенанта), имена их строк НЕ включать в `HostConnectionStringNames`;
   Saga — тенантный. Зафиксировать таблицей в README Mongo.
3. Интеграционные тесты (существующие фикстуры `PostgresDapperFixture`/`MongoFixture`):
   с `Change(tenantA)` соединение открывается к БД тенанта A.

**DoD:** оба DAL тенантны без изменения их кода (или с минимальным async-фиксом); тесты.
