# Cheetah.Core.EntityFramework.Tenants

EF Core реализация мультитенантности «БД-на-тенанта» для абстракций [Cheetah.Core.Tenants](../Cheetah.Core.Tenants/README.md). Хранит реестр тенантов, резолвит контекст под текущего тенанта и мигрирует все tenant-базы. Зависит от `Cheetah.Core`, `CrmEntityFrameworkModule`, `Cheetah.Core.Events`, `CrmTenantsCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmTenantsDbContext` | Контекст реестра тенантов |
| `ITenantBasedDbContext` | Маркер контекста, чьи данные разделены по тенантам |
| `TenantEntityConfiguration<...>` | Базовая EF-конфигурация агрегата тенанта |
| `TenantDatabaseMigrationManager` | Применяет миграции по всем активным тенантам и модулям |
| `ServiceCollectionExtensions`, `ApplicationBuilderExtensions` | Регистрация и инициализация мультитенантности |
| `CrmEntityFrameworkTenantsModule` | Модуль |

## Как работает

1. Модуль приложения реализует `IModuleConnectionStringProvider` (из `Cheetah.Core.Tenants`) — описывает, как назвать БД под тенанта.
2. `CrmTenantsDbContext` хранит список тенантов; `ITenantMigrationService` отдаёт активных тенантов и их строки подключения.
3. `TenantDatabaseMigrationManager` на старте прогоняет миграции по каждой tenant-базе каждого модуля.
4. Контексты, помеченные `ITenantBasedDbContext`, при резолве получают строку подключения текущего тенанта.

См. [Cheetah.Core.Tenants](../Cheetah.Core.Tenants/README.md) для контракта провайдера строк подключения и доменной модели тенанта.
