using Microsoft.AspNetCore.Builder;

namespace Cheetah.Permissions;

public static class EndpointExtensions
{
    /// <summary>
    /// Требует у пользователя указанный permission для доступа к endpoint'у.
    /// Пример: <c>app.MapPost("/contracts/{id}/sign", ...).RequirePermission("Documents.Contract.Sign");</c>
    /// </summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.RequireAuthorization(PermissionPolicyProvider.Prefix + permission);
        return builder;
    }
}
