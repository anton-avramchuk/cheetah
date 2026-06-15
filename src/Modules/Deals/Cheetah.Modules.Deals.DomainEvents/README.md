# Cheetah.Modules.Deals.DomainEvents

Доменные события Deals — чистые контракты (`EventBase`) для интеграции других модулей.
**Без зависимостей**, кроме `Cheetah.Core.Events`: суммы/валюты передаются примитивами, привязки —
`Guid`-ами (не зависит от `Shared`). Другие модули подписываются только на эту сборку (правило
«depend only on Events»).

## Состав

| Событие | Когда публикуется |
|---|---|
| `DealCreatedIntegrationEvent` | Сделка создана |
| `DealStageChangedIntegrationEvent` | Перемещение на другую Open-стадию |
| `DealWonIntegrationEvent` | Сделка выиграна (терминально) |
| `DealLostIntegrationEvent` | Сделка проиграна (терминально) |
| `DealOwnerChangedIntegrationEvent` | Сменился ответственный |

Все публикуются через **Outbox** того же `DealsDbContext` (атомарно с `SaveChangesAsync`).

## Потребители

Workflow (автозадачи при смене стадии), Notification (письмо при `DealWon`), Activities, Timeline,
Search, аналитика.

## Подписка

```csharp
[DependsOn(typeof(CheetahDealsDomainEventsModule))]
public class MyModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var bus = context.ServiceProvider.GetRequiredService<IEventBus>();
        bus.Subscribe<DealWonIntegrationEvent, MyHandler>();
    }
}
```
