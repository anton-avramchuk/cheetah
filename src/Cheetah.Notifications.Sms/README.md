# Cheetah.Notifications.Sms

SMS-канал для [Cheetah.Notifications](../Cheetah.Notifications/README.md). Провайдер-агностичная реализация: POST JSON на настраиваемый HTTP-endpoint.

## Состав

| Тип | Назначение |
|-----|------------|
| `SmsMessage` | Сообщение SMS-канала (`To`, `Body`) |
| `HttpSmsSender` | `INotificationSender<SmsMessage>` — POST `{to, body}` на `SmsOptions.ProviderUrl` |
| `SmsOptions` | `ProviderUrl`, `ApiKey` (опц. Bearer), `Timeout` |
| `SmsDispatcherExtensions.SendSmsAsync` | Удобный extension method поверх `INotificationDispatcher.SendAsync` |
| `CrmNotificationsSmsModule` | Регистрирует sender + named `HttpClient` |

## Подключение

```csharp
[DependsOn(typeof(CrmNotificationsModule))]
[DependsOn(typeof(CrmNotificationsSmsModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"Notifications": {
  "Sms": {
    "ProviderUrl": "https://sms.example/send",
    "ApiKey": "secret-bearer-token",
    "Timeout": "00:00:10"
  }
}
```

## Использование

```csharp
public class AlertService
{
    private readonly INotificationDispatcher _notifications;
    public AlertService(INotificationDispatcher n) => _notifications = n;

    public ValueTask AlertOpsAsync(string phone, string text, CancellationToken ct)
        => _notifications.SendSmsAsync(new SmsMessage(phone, text), ct);
}
```

## Интеграции с конкретными провайдерами

`HttpSmsSender` шлёт `{to, body}` в нашем формате. Для подключения Twilio/SMSC/Aero есть два пути:

1. **Прокси-сервис**: поднимаете маленький сервис, который принимает наш JSON и транслирует в API провайдера. Это удобно — креды провайдера не утекают в основное приложение.
2. **Replace реализацию**:
   ```csharp
   services.Replace(ServiceDescriptor.Scoped<INotificationSender<SmsMessage>, TwilioSmsSender>());
   ```
   `TwilioSmsSender` — отдельный пакет с Twilio SDK.
