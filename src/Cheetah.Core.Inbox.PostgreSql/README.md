# Cheetah.Core.Inbox.PostgreSql

Postgres-специфичные оптимизации для [Cheetah.Core.Inbox](../Cheetah.Core.Inbox/README.md).

## Состав

| Тип | Назначение |
|-----|------------|
| `InboxOptimizedIndexSql` | SQL для создания индекса по `ReceivedAt` (используется cleanup-сервисом) |
| `CrmInboxPostgreSqlModule` | Модуль-маркер, добавляет зависимость на Postgres-слой |

## Применение

Включите SQL в EF-миграцию или выполните вручную при инициализации схемы:

```csharp
public partial class AddInboxOptimizedIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(InboxOptimizedIndexSql.Create());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(InboxOptimizedIndexSql.Drop());
    }
}
```

## Зачем

`InboxCleanupService` периодически вызывает `DeleteOlderThanAsync` —
запрос с фильтром по `ReceivedAt`. Без индекса на больших таблицах это
seq scan; с индексом — bitmap-скан + удаление выбранного диапазона.
