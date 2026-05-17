# Cheetah.DistributedLock

Core-абстракции для **distributed locks** между репликами приложения. Реализация подключается отдельным модулем:

- [Cheetah.DistributedLock.Postgres](../Cheetah.DistributedLock.Postgres/README.md) — `pg_advisory_lock` (рекомендуется)

## Когда нужно

- "Только один инстанс шлёт ночной дайджест"
- Leader election в кластере
- Серийный доступ к внешнему API с строгим rate-limit'ом
- Защита от concurrent migration scripts

**Когда НЕ нужно:**
- Конкуренция за БД-строки — используй `SELECT FOR UPDATE` / optimistic locking
- Идемпотентность handlers — используй `IInboxStore` из `Cheetah.Core.Outbox`
- Очередь задач — используй `Outbox` с `SKIP LOCKED`

Distributed lock — последнее средство. У него высокая цена (round-trip + удерживаемый коннект) и риск deadlock'а.

## Состав

| Тип | Назначение |
|-----|------------|
| `IDistributedLockProvider` | `TryAcquireAsync(key)` (немедленный) и `AcquireAsync(key, timeout)` (с ожиданием) |
| `IDistributedLock` | `IAsyncDisposable` — освобождение при `DisposeAsync` |
| `DistributedLockTimeoutException` | Если `AcquireAsync` не получил за timeout |
| `CrmDistributedLockModule` | Core-модуль |

## Использование

```csharp
public class NightlyDigestService(IDistributedLockProvider locks, ...)
{
    public async Task RunAsync(CancellationToken ct)
    {
        await using var heldLock = await locks.TryAcquireAsync("nightly-digest", ct);
        if (heldLock is null)
        {
            // Другая реплика уже работает
            return;
        }

        // Только мы — гарантированно один на весь кластер
        await SendDigestsAsync(ct);
    }
}
```
