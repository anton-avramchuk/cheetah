using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Auth;

/// <summary>
/// BFF-аутентификация для Blazor Server: cookie для сессии браузера + серверный стор JWT
/// для вызова микросервисов как Bearer. Получение/обновление токена — через шов
/// <see cref="Abstractions.IBffAuthenticator"/> (реализует приложение).
///
/// Хост в Program.cs обязан вызвать (в правильном порядке) <c>UseAuthentication()</c>,
/// <c>UseAuthorization()</c> и зарегистрировать каскадный auth-state для компонентов.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
[DependsOn(typeof(CrmCoreSecurityModule))]
public partial class CrmBlazorAuthModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var services = context.Services;

        services.AddOptions<BffAuthOptions>().BindConfiguration(BffAuthOptions.SectionName);

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, _ => { });

        // Опции cookie настраиваем из BffAuthOptions (после биндинга конфигурации).
        services
            .AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<BffAuthOptions>>((cookie, bff) =>
            {
                var o = bff.Value;
                cookie.Cookie.Name = o.CookieName;
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookie.LoginPath = o.LoginPath;
                cookie.AccessDeniedPath = o.AccessDeniedPath;
                cookie.ExpireTimeSpan = o.ExpireTimeSpan;
                cookie.SlidingExpiration = o.SlidingExpiration;
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();

        // Часы для расчёта истечения токена (переопределяемы в тестах).
        services.TryAddSingleton(TimeProvider.System);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // Эндпоинты входа/выхода. UseRouting уже вызван CrmAspNetCoreModule (зависимость),
        // поэтому route builder доступен.
        var routeBuilder = context.GetRouteBuilder();
        routeBuilder.MapBffAuthEndpoints();
    }
}
