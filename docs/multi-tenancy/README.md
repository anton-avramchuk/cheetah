# Мультитенантность — бэклог реализации

> Основание: [../multi-tenancy.md](../multi-tenancy.md) (проектный документ).
> Здесь — декомпозиция на 33 подзадачи, по файлу на этап.
> Нумерация `T-{этап}.{номер}`. Размер: S — до полудня, M — день, L — 2–3 дня.

## Сквозные требования ко ВСЕМ задачам

- `TreatWarningsAsErrors` (strict build policy), вся солюшн собирается после каждой задачи;
- у каждого затронутого базового модуля (`src/Cheetah.*`) — актуализированный `README.md`
  (pre-commit checklist из CLAUDE.md);
- Conventional Commits, без строки «Generated with Claude Code»;
- новые пакеты — только через `Directory.Packages.props`.

## Этапы

| Этап | Файл | Содержание |
|---|---|---|
| 0 | [stage-0-skeleton-fixes.md](stage-0-skeleton-fixes.md) | Починка существующего скелета (Redis-шина, scope-баг) |
| 1 | [stage-1-current-tenant.md](stage-1-current-tenant.md) | Ядро тенант-контекста: `ICurrentTenant`, `ITenantStore`, клейм, атрибут |
| 2 | [stage-2-encryption.md](stage-2-encryption.md) | `IStringEncryptionService` (AES-256-GCM, ротация ключей) |
| 3 | [stage-3-events.md](stage-3-events.md) | `EventBase.TenantId` + стемпинг/скоуп в четырёх шинах |
| 4 | [stage-4-tenancy-module.md](stage-4-tenancy-module.md) | Модуль `Modules/Tenancy` — каталог тенантов |
| 5 | [stage-5-connection-resolver.md](stage-5-connection-resolver.md) | Тенантный резолвинг строк подключения + кэши |
| 6 | [stage-6-provisioning.md](stage-6-provisioning.md) | Провижининг БД тенантов + реконсиляция |
| 7 | [stage-7-aspnetcore.md](stage-7-aspnetcore.md) | Резолвинг тенанта на входе (middleware, клейм при логине) |
| 8 | [stage-8-background.md](stage-8-background.md) | Фоновые потоки: Outbox, кэш-ключи, межсервисный проброс |
| 9 | [stage-9-pilot.md](stage-9-pilot.md) | Пилот Catalog, интеграционные тесты, регресс, документация |

## Карта зависимостей

```
Этап 0 (починка скелета)  ──────────────┐
Этап 1 (ICurrentTenant)   ──► Этап 3 (события) ──► Этап 8 (фоновые потоки)
        │                 ──► Этап 5 (резолвер строк)
        ▼
Этап 2 (шифрование) ──► Этап 4 (модуль Tenancy) ──► Этап 5 ──► Этап 6 (провижининг)
                                                     │
                                                     ▼
                                          Этап 7 (middleware) ──► Этап 9 (пилот + интеграционные тесты)
```

Параллелить можно: {0.1, 0.2, 1.x, 2.1} — независимы; этап 3 — после 1.1; этап 4 — после 1.2 и 2.1.

## Сводная таблица

| # | Задача | Размер | Зависимости |
|---|---|---|---|
| [T-0.1](stage-0-skeleton-fixes.md#t-01) | Redis-шина: логирование ошибок | S | — |
| [T-0.2](stage-0-skeleton-fixes.md#t-02) | MigrationManager: scope-баг | S | — |
| [T-1.1](stage-1-current-tenant.md#t-11) | ICurrentTenant (AsyncLocal) | M | — |
| [T-1.2](stage-1-current-tenant.md#t-12) | ITenantStore/TenantInfo/Options | S | 1.1 |
| [T-1.3](stage-1-current-tenant.md#t-13) | Клейм tenant_id | S | — |
| [T-1.4](stage-1-current-tenant.md#t-14) | [IgnoreMultiTenancy] | S | — |
| [T-2.1](stage-2-encryption.md#t-21) | IStringEncryptionService (AES-GCM) | M | — |
| [T-3.1](stage-3-events.md#t-31) | EventBase.TenantId + хелперы | S | 1.1 |
| [T-3.2](stage-3-events.md#t-32) | Стемпинг/скоуп в 4 шинах | M | 3.1 |
| [T-4.1](stage-4-tenancy-module.md#t-41) | Tenancy: Events/Shared/Contracts | S | 1.2 |
| [T-4.2](stage-4-tenancy-module.md#t-42) | Tenancy: Domain | L | 4.1 |
| [T-4.3](stage-4-tenancy-module.md#t-43) | Tenancy: Infrastructure | M | 4.2 |
| [T-4.4](stage-4-tenancy-module.md#t-44) | Fix DefaultModuleConnectionStringProvider | S | 4.2 |
| [T-4.5](stage-4-tenancy-module.md#t-45) | Tenancy: Application | L | 4.3, 2.1 |
| [T-4.6](stage-4-tenancy-module.md#t-46) | Tenancy: Api + permissions | M | 4.5 |
| [T-5.1](stage-5-connection-resolver.md#t-51) | Кэш строк + инвалидация | M | 1.2, 4.1 |
| [T-5.2](stage-5-connection-resolver.md#t-52) | TenantConnectionStringResolver | M | 5.1, 1.4 |
| [T-5.3](stage-5-connection-resolver.md#t-53) | Шов в DbContextOptionsFactory | S | 5.2 |
| [T-5.4](stage-5-connection-resolver.md#t-54) | Кэш DbContextOptions | M | 5.3 |
| [T-5.5](stage-5-connection-resolver.md#t-55) | Dapper/Mongo проверка | S | 5.2 |
| [T-6.1](stage-6-provisioning.md#t-61) | Регистрация ITenantBasedDbContext | M | 0.2 |
| [T-6.2](stage-6-provisioning.md#t-62) | Провижининг через стейт | L | 6.1, 4.5 |
| [T-6.3](stage-6-provisioning.md#t-63) | Реконсиляция | M | 6.2 |
| [T-7.1](stage-7-aspnetcore.md#t-71) | Middleware + контрибьюторы | L | 1.1–1.3 |
| [T-7.2](stage-7-aspnetcore.md#t-72) | Прогрев кэша в middleware | S | 7.1, 5.1 |
| [T-7.3](stage-7-aspnetcore.md#t-73) | Identity: клейм при логине | M | 1.3 |
| [T-8.1](stage-8-background.md#t-81) | Outbox per-tenant | L | 3.2, 5.2 |
| [T-8.2](stage-8-background.md#t-82) | Тенант-префикс кэша | M | 1.1 |
| [T-8.3](stage-8-background.md#t-83) | X-Tenant-Id в M2M/gRPC | M | 1.1, 7.1 |
| [T-9.1](stage-9-pilot.md#t-91) | Пилот Catalog | M | 5–7 |
| [T-9.2](stage-9-pilot.md#t-92) | Интеграционные тесты изоляции | L | 9.1 |
| [T-9.3](stage-9-pilot.md#t-93) | Регресс безтенантного режима | S | 9.1 |
| [T-9.4](stage-9-pilot.md#t-94) | Документация | S | всё |

Итого: 33 задачи ≈ 6 L + 12 M + 15 S.
