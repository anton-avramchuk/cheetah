# Cheetah.Core.Outbox.PostgreSql

PostgreSQL **LISTEN/NOTIFY**-нотификатор для [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md). Заменяет polling-задержку: processor просыпается мгновенно после INSERT в `OutboxMessages`.

## Как работает

1. SQL-триггер на `OutboxMessages` шлёт `pg_notify('outbox_new', NEW.Id)` при каждом INSERT.
2. `PostgresOutboxNotifier` (singleton + BackgroundService) держит отдельный `NpgsqlConnection`, выполняет `LISTEN outbox_new`, пинает внутренний `TaskCompletionSource` на каждое уведомление.
3. `OutboxProcessor` ждёт `Task.WhenAny(Task.Delay(PollingInterval), notifier.WaitForSignalAsync())` — что наступит раньше.
4. Если LISTEN-коннект упал, фоновый цикл переподключается с экспоненциальным backoff. Пока коннекта нет — polling работает как fallback.

Polling **не отключается** — `pg_notify` может теряться при переполнении очереди уведомлений (>8GB) и в окне реконнекта.

## Состав

| Тип | Назначение |
|-----|------------|
| `PostgresOutboxNotifier` | `IOutboxNotifier` + `BackgroundService` с долгоживущим LISTEN-коннектом |
| `PostgresOutboxOptions` | `ChannelName`, `ConnectionString`/`ConnectionStringName`, `BaseReconnectDelay`, `MaxReconnectDelay` |
| `OutboxNotifyTriggerSql` | SQL для миграций: `Create()` / `Drop()` |
| `CrmOutboxPostgreSqlModule` | Перекрывает `IOutboxNotifier` с заглушки на `PostgresOutboxNotifier` |

## Подключение

### 1. Модуль

```csharp
[DependsOn(typeof(CrmBackendEventsRedisModule))]
[DependsOn(typeof(CrmOutboxModule))]
[DependsOn(typeof(CrmOutboxEntityFrameworkCoreModule))]
[DependsOn(typeof(CrmOutboxPostgreSqlModule))]    // ← добавь после CrmOutboxModule
public partial class MyApplicationModule : CrmModule { }
```

### 2. Конфигурация

```json
{
  "ConnectionStrings": {
    "Outbox": "Host=localhost;Database=mydb;Username=app;Password=..."
  },
  "Outbox": {
    "PollingInterval": "00:00:30",
    "Postgres": {
      "ChannelName": "outbox_new",
      "ConnectionStringName": "Outbox",
      "BaseReconnectDelay": "00:00:01",
      "MaxReconnectDelay": "00:00:30"
    }
  }
}
```

С LISTEN/NOTIFY `PollingInterval` можно сделать большим (10–30 секунд) — он остаётся только как страховка.

### 3. Миграция с триггером

В EF Core миграции модуля-владельца DbContext:

```csharp
public partial class AddOutboxNotifyTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql(OutboxNotifyTriggerSql.Create());

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql(OutboxNotifyTriggerSql.Drop());
}
```

Имя канала по умолчанию — `outbox_new`. Если меняешь — синхронно в `PostgresOutboxOptions.ChannelName` и в вызове `OutboxNotifyTriggerSql.Create(channelName)`.

## Connection string

Notifier держит **отдельный** коннект (не из EF-пула), потому что LISTEN — долгоживущий. По умолчанию читает `ConnectionStrings:Outbox`. Можно явно задать в `PostgresOutboxOptions.ConnectionString`. Желательно использовать read-only учётку с правом `LISTEN`.

## Tradeoffs

- **+** Задержка публикации стремится к нулю.
- **+** Снижает нагрузку на БД от polling под 10k RPS — таблица не сканируется без нужды.
- **−** Один дополнительный коннект на инстанс приложения.
- **−** Multi-DB сценарий: нужен notifier на каждый DbContext (сейчас зарегистрирован один singleton; для нескольких баз — нужно расширение).
- **−** `pg_notify` ограничен 8000 байт payload и общей очередью 8GB — мы шлём только Id, но в случае флуда возможен дроп. Polling это компенсирует.

## Тестирование

Юнит-тесты есть на `OutboxNotifyTriggerSql` (валидация и формат SQL). Интеграционные тесты на сам LISTEN-цикл требуют реальный Postgres — рекомендуется добавить через `Testcontainers.PostgreSql` (на будущее).
