using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Permissions;

/// <summary>
/// Подключает permissions-инфраструктуру: реестр, authorizer, ASP.NET Core policy provider.
/// Никакой БД. Используйте в каждом модуле / микросервисе, где нужна проверка прав.
///
/// Для хранения каталога permissions подключите дополнительно Cheetah.Permissions.Catalog.
///
/// <see cref="CrmFeatureManagementModule"/> нужен, потому что permission можно привязать к фиче
/// (<c>[Permission(..., Feature = "...")]</c>): выключенная фича отзывает право. Без источника
/// определений флагов работает <c>NullFeatureDefinitionProvider</c> — все флаги выключены, поэтому
/// хосту без фич-флагов не следует привязывать permissions к фичам.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmFeatureManagementModule))]
public partial class CrmPermissionsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.TryAddSingleton<PermissionRegistry>();
        // Scoped, а не Singleton: authorizer спрашивает состояние фичи у scoped IFeatureManager.
        services.TryAddScoped<IPermissionAuthorizer, ClaimPermissionAuthorizer>();
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUserPermissions, CurrentUserPermissions>();

        // ASP.NET Core authorization. Замечание: services.Replace перетирает любой ранее
        // зарегистрированный IAuthorizationPolicyProvider. Подключайте CrmPermissionsModule
        // ПОСЛЕ Identity / другого модуля, который мог зарегистрировать кастомный provider.
        services.AddAuthorization();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());
    }
}
