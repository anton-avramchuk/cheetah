using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Email.Application.EventHandlers;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Email.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Email.Application;

/// <summary>
/// Канал Email: подписан на EmailRequested, отправляет письмо через IEmailGateway и публикует
/// обратный статус (EmailDelivered/EmailFailed). НЕ зависит от Notification — только от своих
/// контрактов; «регистрация провайдера» = эта подписка.
/// </summary>
[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahEmailDomainModule),
    typeof(CheetahEmailDomainEventsModule))]
public partial class CheetahEmailApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<EmailRequested, EmailRequestedHandler>();
    }
}
