# Cheetah.Core.Outbox.Tests

Unit-тесты для [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md).

## Покрытие

**OutboxEventBusTests**
- `PublishAsync` пишет в `IOutboxStore` и не зовёт `IInnerEventBus`
- `PublishManyAsync` добавляет все сообщения
- `Subscribe` делегируется в `IInnerEventBus`
- `OutboxEventSerializer` корректно сериализует/десериализует событие с сохранением `EventId`

**OutboxProcessorTests**
- Pending-сообщения публикуются через `IInnerEventBus` и помечаются `MarkProcessedAsync`
- При ошибке публикации зовётся `MarkFailedAsync` с экспоненциальным backoff (`base * 2^retryCount`)

## Запуск

```bash
dotnet test src/Cheetah.Core.Outbox.Tests/Cheetah.Core.Outbox.Tests.csproj
```
