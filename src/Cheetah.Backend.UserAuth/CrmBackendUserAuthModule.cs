using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.UserAuth;

/// <summary>
/// Подключает on-behalf-of проброс пользовательского токена для исходящих HTTP-вызовов:
/// регистрирует <see cref="UserTokenForwardingHandler"/> и <c>IHttpContextAccessor</c>.
/// </summary>
/// <remarks>
/// Чтобы клиент пробрасывал токен пользователя, повесьте обработчик на его <see cref="HttpClient"/>:
/// <c>.AddHttpMessageHandler&lt;UserTokenForwardingHandler&gt;()</c> (или <c>.AddUserTokenForwarding()</c>)
/// и сделайте модуль клиента зависимым от этого модуля.
/// </remarks>
[DependsOn(typeof(CoreModule))]
public partial class CrmBackendUserAuthModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddHttpContextAccessor();
    }
}
