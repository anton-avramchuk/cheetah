# Cheetah.Core.Outbox.OpenTelemetry.Tests

Unit-тесты для [Cheetah.Core.Outbox.OpenTelemetry](../Cheetah.Core.Outbox.OpenTelemetry/README.md).

Проверяет:
- `RecordPublished` отдаёт измерения через `MeterListener` с тегом `event_type`
- `RecordFailed` проставляет тег `dead_letter`

## Запуск

```bash
dotnet test src/Cheetah.Core.Outbox.OpenTelemetry.Tests/Cheetah.Core.Outbox.OpenTelemetry.Tests.csproj
```
