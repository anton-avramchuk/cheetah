using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Email.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Email.Infrastructure.Gateways;

/// <summary>
/// Dev-шлюз: вместо реальной отправки логирует письмо и возвращает успех с псевдо-ID.
/// Заглушка для слайса — реальный SMTP/SES-адаптер (с ретраями, rate limiting, обработкой
/// провайдерских ошибок) — follow-up. Подменяется регистрацией другого IEmailGateway.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IEmailGateway))]
public sealed class LoggingEmailGateway : IEmailGateway
{
    private readonly ILogger<LoggingEmailGateway> _logger;

    public LoggingEmailGateway(ILogger<LoggingEmailGateway> logger) => _logger = logger;

    public ValueTask<EmailSendResult> SendAsync(
        string toAddress,
        string subject,
        string htmlBody,
        string? plainBody,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] to={To} subject=\"{Subject}\" idempotencyKey={Key}\n{Body}",
            toAddress, subject, idempotencyKey, htmlBody);

        var providerMessageId = $"dev-{idempotencyKey}";
        return ValueTask.FromResult(EmailSendResult.Ok(providerMessageId));
    }
}
