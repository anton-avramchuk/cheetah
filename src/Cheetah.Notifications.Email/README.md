# Cheetah.Notifications.Email

SMTP-канал для [Cheetah.Notifications](../Cheetah.Notifications/README.md). Реализация поверх `System.Net.Mail.SmtpClient` (BCL).

## Состав

| Тип | Назначение |
|-----|------------|
| `EmailMessage` | Сообщение email-канала (`To`, `Subject`, `Body`, `IsHtml`, `Cc`, `Bcc`, `From`, `Attachments`) |
| `Attachment` | Вложение (FileName, ContentType, Content) |
| `SmtpEmailSender` | `INotificationSender<EmailMessage>` через `System.Net.Mail` |
| `SmtpOptions` | `Host`, `Port`, `EnableSsl`, `Username`, `Password`, `DefaultFromAddress`, `DefaultFromDisplayName`, `Timeout` |
| `EmailDispatcherExtensions.SendEmailAsync` | Удобный extension method поверх `INotificationDispatcher.SendAsync` |
| `CrmNotificationsEmailModule` | Регистрирует sender + Configure из секции `Notifications:Smtp` |

## Подключение

```csharp
[DependsOn(typeof(CrmNotificationsModule))]
[DependsOn(typeof(CrmNotificationsEmailModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"Notifications": {
  "Smtp": {
    "Host": "smtp.yandex.ru",
    "Port": 465,
    "EnableSsl": true,
    "Username": "no-reply@example.com",
    "Password": "...",
    "DefaultFromAddress": "no-reply@example.com",
    "DefaultFromDisplayName": "Acme CRM",
    "Timeout": "00:00:30"
  }
}
```

## Замена реализации (MailKit, SendGrid, ...)

`System.Net.Mail.SmtpClient` достаточен для простых сценариев, но не поддерживает modern OAuth2 и плохо работает с некоторыми ESP. Для прод-нагрузки рекомендуется подменить реализацию:

```csharp
// В ConfigureServices модуля
services.Replace(ServiceDescriptor.Scoped<INotificationSender<EmailMessage>, MailKitEmailSender>());
```

`MailKitEmailSender` — отдельный пакет, который вы добавите сами (или мы выделим как `Cheetah.Notifications.Email.MailKit` при необходимости).

## Вложения

```csharp
new EmailMessage {
    To = new[] { "user@example.com" },
    Subject = "Договор",
    Body = "Во вложении.",
    Attachments = new[] {
        new Attachment("contract.pdf", "application/pdf", File.ReadAllBytes("contract.pdf"))
    }
}
```
