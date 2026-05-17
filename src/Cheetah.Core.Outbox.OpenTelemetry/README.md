# Cheetah.Core.Outbox.OpenTelemetry

OpenTelemetry-метрики для [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md).

## Зачем отдельный пакет

`Cheetah.Core.Outbox` не тянет за собой никаких метрических зависимостей — там только интерфейс `IOutboxMetrics` и no-op реализация. Проекты, которым метрики не нужны, не платят за них ничем. Тот, кому нужны — подключает этот модуль.

## Состав

| Тип | Назначение |
|-----|------------|
| `OpenTelemetryOutboxMetrics` | Реализация `IOutboxMetrics` поверх `System.Diagnostics.Metrics.Meter` |
| `MeterName` | `"Cheetah.Core.Outbox"` — используйте в `AddMeter(...)` |
| `CrmOutboxOpenTelemetryModule` | Заменяет `NullOutboxMetrics` (из `CrmOutboxModule`) на реальную реализацию |

## Подключение

```csharp
[DependsOn(typeof(CrmOutboxModule))]
[DependsOn(typeof(CrmOutboxOpenTelemetryModule))]   // ← после CrmOutboxModule
public partial class MyApplicationModule : CrmModule { }
```

И зарегистрировать meter в OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter(OpenTelemetryOutboxMetrics.MeterName));
```

## Экспортируемые метрики

| Имя | Тип | Теги | Описание |
|-----|-----|------|----------|
| `outbox.published` | Counter | `event_type` | Успешные публикации |
| `outbox.failed` | Counter | `event_type`, `dead_letter` | Ошибки публикации (с пометкой DLQ) |
| `outbox.publish_latency` | Histogram (ms) | `event_type` | Латентность publish → markProcessed |
| `outbox.cleaned` | Counter | `table` | Строки, удалённые cleanup-сервисом |
