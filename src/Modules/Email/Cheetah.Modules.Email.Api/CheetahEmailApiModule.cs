using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Email.DomainEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Email.Api;

/// <summary>
/// Технический эндпоинт приёма асинхронных статусов от почтового провайдера (bounce/complaint).
/// Это НЕ публичный контракт для других .NET-модулей (Client не нужен) — только вход вебхуков.
/// Опубликованный EmailBounced ловит Notification и помечает Dispatch как Bounced.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahEmailDomainEventsModule),
    typeof(Application.CheetahEmailApplicationModule))]
public partial class CheetahEmailApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routes = context.GetRouteBuilder();

        routes.MapPost("api/email/webhooks/{provider}", HandleWebhookAsync)
            .WithName("EmailProviderWebhook").WithTags("Email");
    }

    private static async Task<IResult> HandleWebhookAsync(
        [FromRoute] string provider,
        [FromBody] EmailWebhookEvent payload,
        [FromServices] IEventBus eventBus,
        CancellationToken ct)
    {
        // TODO(follow-up): верификация подписи провайдера и маппинг его формата в EmailWebhookEvent.
        await eventBus.PublishAsync(
            new EmailBounced(payload.DispatchId, payload.ToAddress, payload.Type, DateTimeOffset.UtcNow), ct);

        return Results.Ok();
    }
}
