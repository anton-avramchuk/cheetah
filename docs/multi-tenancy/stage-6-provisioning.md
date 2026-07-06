# Этап 6. Провижининг БД тенантов

> [← Бэклог](README.md) · Зависимости: [T-0.2](stage-0-skeleton-fixes.md#t-02),
> [T-4.5](stage-4-tenancy-module.md#t-45).

<a id="t-61"></a>
## T-6.1 Починка регистрации `ITenantBasedDbContext` — **M**

**Зависит от:** T-0.2.
**Файлы:** `src/Cheetah.Core.EntityFramework.Tenants/Extensions/ServiceCollectionExtensions.cs`,
`ITenantBasedDbContext.cs`, новый `TenantBasedDbContextPrototype.cs`.

**Проблема.** `AddTenantsDbContext` регистрирует только сам DbContext;
`TenantDatabaseMigrationManager` резолвит `IEnumerable<ITenantBasedDbContext<...>>` и
всегда получает пусто — механизм мёртвый.

**Шаги:**
1. Ввести generic-прототип, чтобы модулю не писать руками `CreateForTenant`:
   ```csharp
   internal sealed class TenantBasedDbContextPrototype<TDbContext> : ITenantBasedDbContext
       where TDbContext : DbContext, ICrmDbContext
   {
       public string ModuleName { get; }
       public DbContext CreateForTenant(string connectionString) { /* new DbContextOptionsBuilder<TDbContext>().UseNpgsql(cs)... через провайдер-делегат */ }
   }
   ```
   Заодно упростить сам `ITenantBasedDbContext`: убрать generic-параметр
   `TTenantCreatedEvent` (он там не используется по назначению) — интерфейсу достаточно
   `ModuleName` + фабрики. Событийная привязка остаётся у менеджера миграций.
2. Новый экстеншен для модулей:
   `services.AddTenantBasedDbContext<TDbContext>(moduleName)` — регистрирует прототип
   (singleton) + `DefaultModuleConnectionStringProvider(moduleName)` (если модуль не дал
   кастомный). Это ЕДИНСТВЕННОЕ, что бизнес-модуль делает для входа в мультитенантность.
3. Провайдер СУБД для прототипа: делегат конфигурации (`UseNpgsql`) забирать из
   существующей инфраструктуры `CrmDbContextOptions.ConfigureActions`, не хардкодить
   Npgsql в Tenants-сборке.
4. Тесты: после `AddTenantBasedDbContext<FooDbContext>("Foo")` менеджер видит один
   прототип; `CreateForTenant` возвращает контекст с нужной строкой.

**DoD:** механизм регистрации живой end-to-end (unit-уровень).

<a id="t-62"></a>
## T-6.2 `TenantDatabaseMigrationManager` → через провижининг-стейт — **L**

**Зависит от:** T-6.1, T-4.5.
**Файлы:** `src/Cheetah.Core.EntityFramework.Tenants/Migrations/TenantDatabaseMigrationManager.cs`.

**Шаги:**
1. Консюмер `TenantCreatedEvent` (уже есть) дорабатывается:
   - идемпотентность: `MigrateAsync` сам по себе идемпотентен; повторная доставка события
     безопасна — зафиксировать тестом;
   - по каждому контексту: успех → `MarkModuleProvisionedCommand` (через `IDispatcher`),
     провал → `MarkModuleProvisioningFailedCommand` + продолжить остальные модули
     (сейчас `throw` обрывает цикл на первом — одна сломанная БД не должна блокировать
     остальные);
   - ретраи: 3 попытки с экспоненциальной задержкой на transient-ошибки
     (`NpgsqlException.IsTransient`), затем Failed.
2. В микросервисном режиме каждый сервис держит своего менеджера и мигрирует только свои
   контексты (у него других и нет) — код одинаковый, отметить в README.
3. Монолит-оптимизация: `CreateTenantCommandHandler` может (опция) вызвать провижининг
   синхронно сразу после коммита — пользователь получает готового тенанта без ожидания
   консюмера. Событие всё равно публикуется (микросервисы, аудит). Реализовать за опцией
   `Multitenancy:ProvisionSynchronously` (default true для монолита).
4. Тесты: частичный сбой (2 модуля ок, 1 упал) → статусы корректные, остальные не
   заблокированы; повторное событие не ломает; ретраи работают.

**DoD:** провижининг наблюдаемый (статусы), частично-отказоустойчивый, идемпотентный.

<a id="t-63"></a>
## T-6.3 Реконсиляция на старте и по расписанию — **M**

**Зависит от:** T-6.2.
**Файлы:** `src/Cheetah.Core.EntityFramework.Tenants/` → `TenantProvisioningReconciler.cs`
(BackgroundService); заменяет `ApplicationBuilderExtensions.MigrateTenantDatabases`.

**Назначение.** Закрывает два сценария: (а) потерянное/недоставленное событие создания;
(б) в систему добавили новый мультитенантный модуль — у существующих тенантов нет его БД.

**Шаги:**
1. `BackgroundService`: на старте (после инициализации приложения, НЕ блокируя readiness)
   и далее по `MultitenancyOptions.ReconciliationInterval` (null = только на старте):
   - активные тенанты × зарегистрированные прототипы контекстов;
   - для пары без записи провижининга — создать `Pending` (это и есть «новый модуль»);
   - для `Pending`/`Failed` (с backoff по `LastAttemptAt`, чтобы не молотить постоянно
     падающий модуль) — прогнать миграцию, обновить статус;
   - `Provisioned` — пропустить (обычные миграции при деплое новой версии прогоняет
     тот же цикл: `MigrateAsync` на Provisioned-паре дешёв, если миграций нет — решить:
     на старте прогонять ВСЕ пары один раз, это заменяет старый `MigrateTenantDatabases`).
2. Ограниченный параллелизм: `SemaphoreSlim(4)` поверх пар (тенант, модуль); порядок —
   по тенантам, чтобы тенант становился Active как можно раньше.
3. Distributed lock (существующий `DistributedLock` из инфраструктуры Booking) на цикл
   реконсиляции — два инстанса не должны мигрировать одну БД одновременно.
4. Старый `MigrateTenantDatabases()`-экстеншен пометить `[Obsolete]` → удалить.
5. Тесты: новый модуль у старого тенанта получает БД; Failed ретраится с backoff;
   параллелизм ограничен (ассерт по максимуму одновременных вызовов на стабах).

**DoD:** старт приложения самодостаточен: после него все активные тенанты имеют все БД
всех модулей; readiness не блокируется.
