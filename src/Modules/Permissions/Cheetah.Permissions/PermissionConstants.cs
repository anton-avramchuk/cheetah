namespace Cheetah.Permissions;

/// <summary>
/// Имена claim-типов, используемых модулем Permissions. Совместимо с ASP.NET Core Identity:
/// permissions хранятся как claim'ы вида (PermissionClaimType, "ModuleName.Action") на роли
/// (через IdentityRoleClaim) или прямо на пользователе (IdentityUserClaim).
/// </summary>
public static class PermissionConstants
{
    /// <summary>
    /// Тип claim'а для permission. Например: ("permission", "Documents.Contract.Sign").
    /// </summary>
    public const string PermissionClaimType = "permission";
}
