using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notification.Application.Notifications;
using Cheetah.Modules.Notification.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Notification.Api;

[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(Application.CheetahNotificationApplicationModule),
    typeof(Contracts.CheetahNotificationContractsModule))]
public partial class CheetahNotificationApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routes = context.GetRouteBuilder();

        // Ручной триггер уведомления (стенд продюсера). В проде это событие шлют сами модули.
        routes.MapPost("api/notifications", RequestNotificationAsync)
            .WithName("RequestNotification").WithTags("Notifications");

        // История уведомлений получателя
        routes.MapGet("api/notifications/{userId:guid}/history", GetHistoryAsync)
            .WithName("GetNotificationHistory").WithTags("Notifications");
    }

    private static async Task<IResult> RequestNotificationAsync(
        [FromBody] RequestNotificationRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<RequestNotificationCommand, Guid>(
            new RequestNotificationCommand(
                request.RecipientUserId, request.TemplateKey, request.Category, request.Data, request.ForceChannel),
            ct);

        // Доставка асинхронна — возвращаем 202 с идентификатором уведомления.
        return Results.Accepted($"/api/notifications/{request.RecipientUserId}/history", new { notificationId = id });
    }

    private static async Task<IResult> GetHistoryAsync(
        [FromRoute] Guid userId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<GetNotificationHistoryQuery, IReadOnlyList<NotificationHistoryDto>>(
            new GetNotificationHistoryQuery(userId), ct);
        return Results.Ok(items);
    }
}
