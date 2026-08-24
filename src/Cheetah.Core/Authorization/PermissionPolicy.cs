namespace Cheetah.Core.Authorization;

/// <summary>
/// Конвенция имени политики авторизации «требуется право X»: <c>permission:{право}</c>.
/// Живёт в ядре, потому что имя политики совместно используют две несвязанные стороны —
/// модуль <c>Cheetah.Permissions</c>, который такие политики выдаёт по запросу
/// (<c>PermissionPolicyProvider</c>), и генератор эндпоинтов, который вешает их на маршруты
/// по <c>EndpointConfiguration.RequirePermissions(...)</c>. Строка обязана совпадать у обоих.
/// </summary>
public static class PermissionPolicy
{
    /// <summary>Префикс имени политики.</summary>
    public const string Prefix = "permission:";

    /// <summary>Имя политики для конкретного права.</summary>
    public static string For(string permission) => Prefix + permission;
}
