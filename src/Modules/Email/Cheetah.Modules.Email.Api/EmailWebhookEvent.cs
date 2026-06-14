using Cheetah.Modules.Email.DomainEvents;

namespace Cheetah.Modules.Email.Api;

/// <summary>
/// Нормализованный payload вебхука провайдера. Реальный адаптер (SES SNS / Twilio / FCM)
/// маппит провайдер-специфичный формат в эту форму — см. follow-up.
/// </summary>
public sealed record EmailWebhookEvent(
    Guid DispatchId,
    string ToAddress,
    BounceType Type);
