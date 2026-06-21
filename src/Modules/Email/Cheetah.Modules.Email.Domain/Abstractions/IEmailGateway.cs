using Cheetah.Modules.Email.DomainEvents;

namespace Cheetah.Modules.Email.Domain.Abstractions;

/// <summary>Результат попытки отправки письма через внешний шлюз.</summary>
/// <param name="Success">Принято ли письмо шлюзом.</param>
/// <param name="ProviderMessageId">Идентификатор сообщения у провайдера (при успехе).</param>
/// <param name="Reason">Причина неудачи (при провале).</param>
/// <param name="IsPermanent">Постоянная ли неудача (true → не повторять, фолбэк/стоп).</param>
/// <param name="Detail">Диагностическое сообщение.</param>
public sealed record EmailSendResult(
    bool Success,
    string? ProviderMessageId,
    EmailFailureReason? Reason,
    bool IsPermanent,
    string? Detail)
{
    public static EmailSendResult Ok(string providerMessageId) => new(true, providerMessageId, null, false, null);

    public static EmailSendResult Fail(EmailFailureReason reason, bool isPermanent, string? detail) =>
        new(false, null, reason, isPermanent, detail);
}

/// <summary>
/// Порт к внешнему почтовому шлюзу (SMTP/SES/...). Реализация — в Infrastructure.
/// <c>idempotencyKey</c> = DispatchId: передаётся провайдеру, чтобы дедуплицировать
/// отправку на его стороне.
/// </summary>
public interface IEmailGateway
{
    ValueTask<EmailSendResult> SendAsync(
        string toAddress,
        string subject,
        string htmlBody,
        string? plainBody,
        string idempotencyKey,
        CancellationToken ct = default);
}
