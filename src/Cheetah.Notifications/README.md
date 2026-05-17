# Cheetah.Notifications

Core-абстракции для уведомлений. Никаких провайдеров здесь нет — только контракты, фасад и Null-реализация. Конкретные каналы подключаются отдельными модулями:

- [Cheetah.Notifications.Email](../Cheetah.Notifications.Email/README.md) — SMTP
- [Cheetah.Notifications.Sms](../Cheetah.Notifications.Sms/README.md) — HTTP webhook

## Состав

| Тип | Назначение |
|-----|------------|
| `INotificationSender<TMessage>` | Generic-sender одного канала; каждый канальный модуль регистрирует свою реализацию |
| `INotificationDispatcher` / `NotificationDispatcher` | Фасад: `SendAsync<TMessage>` резолвит подходящий sender через DI |
| `NullNotificationSender<TMessage>` | No-op sender для dev/test |
| `CrmNotificationsModule` | Регистрирует диспетчер |

> **Дизайн**: Core ничего не знает о конкретных каналах. `EmailMessage`, `SmsMessage` и удобные методы `SendEmailAsync` / `SendSmsAsync` живут в своих канальных модулях как extension methods. Добавите новый канал — Core не меняется.

## Использование

```csharp
[DependsOn(typeof(CrmNotificationsModule))]
[DependsOn(typeof(CrmNotificationsEmailModule))]   // подключаем нужные каналы
[DependsOn(typeof(CrmNotificationsSmsModule))]
public partial class MyAppModule : CrmModule { }
```

```csharp
public class WelcomeService
{
    private readonly INotificationDispatcher _notifications;
    public WelcomeService(INotificationDispatcher n) => _notifications = n;

    public async ValueTask SendWelcomeAsync(string email, CancellationToken ct)
    {
        await _notifications.SendEmailAsync(new EmailMessage
        {
            To = new[] { email },
            Subject = "Добро пожаловать",
            Body = "<h1>Hi!</h1>",
            IsHtml = true
        }, ct);
    }
}
```

## Reliability через Outbox

Прямой вызов `SendEmailAsync` упадёт, если SMTP временно недоступен. Для гарантированной доставки рекомендуется паттерн:

1. В command handler'е публикуем событие `WelcomeEmailRequested` через `IEventBus` (которое пишется в outbox);
2. EventHandler (через Source Generator с `[Idempotent]`) подписывается и вызывает `INotificationDispatcher.SendEmailAsync`;
3. Если падает — outbox обеспечивает retry, inbox — идемпотентность.

См. [Cheetah.Core.Outbox](../Cheetah.Core.Outbox/README.md).
