# Cheetah.Core.Outbox.Integration.Tests

End-to-end интеграционные тесты для [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md) на **реальном Postgres** через [Testcontainers](https://testcontainers.com/).

## Что покрыто

| Тест | Что проверяет |
|------|---------------|
| `EfOutboxStore_сохраняет_и_возвращает_pending` | `AddAsync` → `SaveChanges` → `GetPendingAsync` возвращает сообщение; после `MarkProcessedAsync` оно исчезает |
| `PostgresOutboxStore_SKIP_LOCKED_не_отдаёт_одно_сообщение_двум_репликам` | Параллельный `GetPendingAsync` из двух `DbContext` не пересекается, обе пачки покрывают 10 сообщений |
| `DeleteProcessedAsync_удаляет_только_старые_обработанные` | Cleanup удаляет только `ProcessedAt < threshold`, не трогает recent processed и pending |
| `LISTEN_NOTIFY_сигналит_быстрее_polling_intervala` | `pg_notify('outbox_new', ...)` будит `WaitForSignalAsync` менее чем за 2 секунды |
| `INSERT_в_OutboxMessages_триггерит_NOTIFY` | SQL-триггер из `OutboxNotifyTriggerSql.Create()` реально срабатывает на INSERT через EF |

## Требования

- **Docker Desktop** запущен (Linux/Windows-engine). Тесты автоматически стартуют контейнер `postgres:16-alpine`.
- Время первого прогона: ~15 сек (pull image + start container).

## Запуск

```bash
dotnet test src/Cheetah.Core.Outbox.Integration.Tests/Cheetah.Core.Outbox.Integration.Tests.csproj
```

Если Docker недоступен — тесты упадут на этапе `_container.StartAsync()`. Их разумно гейтить env-variable или отдельной CI-job-ой.

## Структура

- `PostgresFixture` — `IAsyncLifetime`, поднимает контейнер один раз на класс (через `ICollectionFixture<PostgresFixture>` — на всю сборку), создаёт схему через `EnsureCreatedAsync` и применяет `OutboxNotifyTriggerSql.Create()`.
- `TestDbContext` — минимальный DbContext, реализующий `IOutboxDbContext` + `IInboxDbContext`, вызывает `modelBuilder.AddOutbox().AddInbox()`.
