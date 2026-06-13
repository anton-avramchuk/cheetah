# Cheetah.CQRS.Dispatcher

Standalone-реализация `IDispatcher` из [Cheetah.Core.CQRS](../Cheetah.Core.CQRS/README.md). Альтернатива [Cheetah.Backend.CQRS](../Cheetah.Backend.CQRS/README.md) без привязки к backend-обвязке. Зависит от `Cheetah.Core`, `CrmCQRSCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `Dispatcher` (`IDispatcher`, Scoped) | Резолвит `ICommandHandler<,>` / `IQueryHandler<,>` из DI и делегирует `HandleAsync` |
| `CrmCQRSDispatcherModule` | Модуль |

Функционально совпадает с `Cheetah.Backend.CQRS.Dispatcher`. Подключайте этот модуль в окружениях, где backend-обвязка не нужна (например, фоновые сервисы, тесты), и `Cheetah.Backend.CQRS` — в полноценном API-хосте. Одновременно подключать обе реализации `IDispatcher` не следует.

## Подключение

```csharp
[DependsOn(typeof(CrmCQRSDispatcherModule))]
public partial class MyWorkerModule : CrmModule { }
```
