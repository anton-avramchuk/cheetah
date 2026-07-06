# Этап 0. Починка существующего скелета

> [← Бэклог](README.md) · Зависимости: нет — можно начинать сразу, параллельно с этапами 1–2.

<a id="t-01"></a>
## T-0.1 CrmRedisEventBus: логирование и политика ошибок хендлеров — **S**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Backend.Events.Redis/CrmRedisEventBus.cs`.

**Проблема.** `HandleEventAsync` глотает исключения хендлеров (`catch { }` с TODO). Любой
упавший подписчик (в будущем — провижининг тенанта) теряется молча.

**Шаги:**
1. Внедрить `ILogger<CrmRedisEventBus>` в конструктор.
2. В `catch` — `logger.LogError(ex, "Event handler {HandlerType} failed for {EventType} {EventId}", ...)`.
   Поведение «продолжить остальные хендлеры» сохранить (это осознанно), но ошибку фиксировать.
3. Симметрично проверить `CrmKafkaEventBus` и `InMemoryEventBus` — если там тот же паттерн,
   поправить в этой же задаче.
4. Юнит-тест: хендлер бросает → второй хендлер всё равно вызван, ошибка залогирована
   (через `FakeLogger`/`ILogger`-стаб).

**DoD:** ни одна реализация `IEventBus` не глотает исключения без лога; тесты зелёные.

<a id="t-02"></a>
## T-0.2 TenantDatabaseMigrationManager: scope-баг — **S**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Core.EntityFramework.Tenants/Migrations/TenantDatabaseMigrationManager.cs`.

**Проблема.** В `MigrateTenantDatabasesAsync`/`CreateTenantDatabasesAsync` scope создаётся,
но `ITenantBasedDbContext` резолвится из root-провайдера (`serviceProvider.GetServices(...)`
вместо `scope.ServiceProvider.GetServices(...)`).

**Шаги:**
1. Резолвить прототипы контекстов из `scope.ServiceProvider`.
2. Устранить дублирование: `CreateTenantDatabasesAsync` и `MigrateTenantDatabasesAsync`
   отличаются только текстами логов — слить в один приватный метод (у `MigrateAsync`
   семантика «создай, если нет, и накати» — оба сценария покрывает).

**DoD:** один общий код-путь; резолвинг только из scope.
