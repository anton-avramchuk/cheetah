# Cheetah.Core.Tenants

Core-абстракции мультитенантности на уровне **БД-на-тенанта**. Каждый модуль может иметь свою базу под каждого тенанта; этот модуль отвечает за генерацию строк подключения и доступ к списку тенантов при миграциях. EF-реализация — в [Cheetah.Core.EntityFramework.Tenants](../Cheetah.Core.EntityFramework.Tenants/README.md). Зависит от `Cheetah.Core`, `Cheetah.Core.Domain`, `Cheetah.Core.Events`.

## Состав

| Тип | Назначение |
|-----|------------|
| `IModuleConnectionStringProvider` | Реализуется модулем, которому нужна tenant-specific БД: `ModuleName` + `GenerateConnectionString(tenantId, tenantName, baseTemplate)` |
| `ITenantConnectionStringService` | Генерирует строки подключения по всем зарегистрированным модулям для нового тенанта |
| `ITenantMigrationService` | Доступ к активным тенантам и их строкам подключения во время миграций |
| `TenantMigrationInfo` | `record (Guid Id, string Name)` |
| `DefaultModuleConnectionStringProvider` | Дефолтная реализация провайдера |
| `TenantEntity<...>` | База агрегата тенанта (Name, Description, IsActive, аудит); параметризуется доменными событиями |
| `TenantCreated/Updated/Deactivated/Activated`-события | Контракты доменных событий тенанта |

## Как подключить модуль к мультитенантности

Модуль, которому нужна отдельная БД на тенанта, регистрирует свой провайдер:

```csharp
[Export(LifetimeType.Singleton, typeof(IModuleConnectionStringProvider))]
public class IdentityConnectionStringProvider : IModuleConnectionStringProvider
{
    public string ModuleName => "Identity";

    public string GenerateConnectionString(Guid tenantId, string tenantName, string baseConnectionString)
        => new NpgsqlConnectionStringBuilder(baseConnectionString)
           {
               Database = $"identity_{tenantName}"
           }.ToString();
}
```

При создании тенанта `ITenantConnectionStringService` опрашивает все такие провайдеры и формирует полный набор строк подключения; `ITenantMigrationService` затем применяет миграции к каждой tenant-базе.
