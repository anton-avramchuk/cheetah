using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Permissions;

/// <summary>
/// Подключает permissions-инфраструктуру: реестр, authorizer, ASP.NET Core policy provider.
/// Никакой БД. Используйте в каждом модуле / микросервисе, где нужна проверка прав.
///
/// Для хранения каталога permissions подключите дополнительно Cheetah.Permissions.Catalog.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmPermissionsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.TryAddSingleton<PermissionRegistry>();
        services.TryAddSingleton<IPermissionAuthorizer, ClaimPermissionAuthorizer>();
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUserPermissions, CurrentUserPermissions>();

        // ASP.NET Core authorization. Замечание: services.Replace перетирает любой ранее
        // зарегистрированный IAuthorizationPolicyProvider. Подключайте CrmPermissionsModule
        // ПОСЛЕ Identity / другого модуля, который мог зарегистрировать кастомный provider.
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());
    }
}
