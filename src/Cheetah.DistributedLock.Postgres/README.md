# Cheetah.DistributedLock.Postgres

Реализация [Cheetah.DistributedLock](../Cheetah.DistributedLock/README.md) через **PostgreSQL advisory locks**.

## Почему Postgres, а не Redis

`pg_advisory_lock` — session-scoped lock. При разрыве connection блокировка **автоматически освобождается на сервере**. Никаких TTL, никаких "lock expired but client still thinks it holds it" — нет split-brain'а при failover'е.

Сравните с Redis RedLock — он популярен, но [Martin Kleppmann показал](https://martin.kleppmann.com/2016/02/08/how-to-do-distributed-locking.html), что его корректность сомнительна при failover'е/GC-паузах: lock может истечь по TTL пока клиент думает, что удерживает его. Для критичных задач (миграции, leader election) это неприемлемо.

Цена pg-advisory: каждый удерживаемый lock = одна открытая connection. Не злоупотребляй долгими блокировками.

## Состав

| Тип | Назначение |
|-----|------------|
| `PostgresDistributedLockProvider` | Реализация `IDistributedLockProvider`. Через `pg_try_advisory_lock(bigint)` |
| `PostgresLockOptions` | `ConnectionString` / `ConnectionStringName`, `RetryInterval` |
| `CrmDistributedLockPostgresModule` | Зависит от `CrmDistributedLockModule` |

## Подключение

```csharp
[DependsOn(typeof(CrmDistributedLockModule))]
[DependsOn(typeof(CrmDistributedLockPostgresModule))]
public partial class MyAppModule : CrmModule { }
```

```json
{
  "ConnectionStrings": {
    "DistributedLock": "Host=localhost;Database=app;Username=...;Password=..."
  },
  "DistributedLock": {
    "Postgres": {
      "RetryInterval": "00:00:00.200"
    }
  }
}
```

> **Рекомендация**: используй отдельную connection-string для locks (или ту же что у основной БД — нет разницы, advisory locks не конкурируют с обычными запросами). Главное — пул подключений должен иметь запас под максимальное число одновременно удерживаемых locks.

## Внутренности

1. **Хеширование ключа.** `pg_advisory_lock` принимает `bigint`. String-ключ хешируется через SHA-256 (первые 8 байт → int64). Вероятность коллизий ≈ 2⁻³² для случайных пар ключей — приемлемо для типичных нагрузок (десятки lock-имён). Если хочется ноль коллизий — используй явные числовые ключи и шли их без хеширования.

2. **TryAcquireAsync.** Открывает `NpgsqlConnection`, выполняет `SELECT pg_try_advisory_lock(@hash)`. Если `true` — возвращает `IDistributedLock`, который держит коннект до `DisposeAsync`. Если `false` — закрывает коннект и возвращает `null`.

3. **AcquireAsync(timeout).** Loop из TryAcquire + `Task.Delay(RetryInterval)` до получения lock'а или истечения timeout. Не используем blocking `pg_advisory_lock` (без `try`), потому что он блокирует connection на сервере без явного timeout'а.

4. **DisposeAsync.** Вызывает `pg_advisory_unlock(@hash)` явно, затем закрывает коннект. Если unlock упал (например, коннект уже отвалился) — не критично, server-side release происходит автоматически.

## Tradeoffs

- **+** Корректность под failover'ом — auto-release при разрыве сессии
- **+** Нет TTL'ов — не нужно угадывать "сколько у нас займёт работа"
- **+** Простая операционка — locks видны через `pg_locks` для диагностики
- **−** Один lock = одна connection. Pool size надо планировать
- **−** Только Postgres. Если у тебя CRM на MS SQL — есть `sp_getapplock`, MySQL — `GET_LOCK()` (можно добавить аналогичные модули)
- **−** Хеширование строковых ключей — теоретическая коллизия. Для большинства приложений нерелевантно
