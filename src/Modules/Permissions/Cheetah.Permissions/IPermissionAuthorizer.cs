using System.Security.Claims;
using Cheetah.FeatureManagement;

namespace Cheetah.Permissions;

/// <summary>
/// Проверка прав по ClaimsPrincipal. Claims приходят из JWT, который Identity подкладывает
/// при логине (IdentityRoleClaim'ы пользовательских ролей + IdentityUserClaim'ы).
/// <para>
/// Кроме claims учитывается фич-флаг, к которому привязан permission
/// (<c>[Permission(..., Feature = "...")]</c>): пока фича выключена, право не действует, даже
/// если claim выдан. Поэтому проверка асинхронна — движок флагов асинхронен.
/// </para>
/// <para>
/// Если нужна более сложная логика (проверка scope, иерархия permissions) —
/// реализуйте через собственную реализацию <see cref="IPermissionAuthorizer"/>.
/// </para>
/// </summary>
public interface IPermissionAuthorizer
{
    /// <summary>Истинно, если у пользователя есть указанный permission и его фича включена.</summary>
    ValueTask<bool> HasAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default);

    /// <summary>Истинно, если у пользователя есть ВСЕ указанные permissions.</summary>
    ValueTask<bool> HasAllAsync(ClaimsPrincipal user, IEnumerable<string> permissions, CancellationToken ct = default);

    /// <summary>Истинно, если у пользователя есть ХОТЯ БЫ ОДИН из указанных permissions.</summary>
    ValueTask<bool> HasAnyAsync(ClaimsPrincipal user, IEnumerable<string> permissions, CancellationToken ct = default);
}

/// <summary>
/// Claims + фич-флаги. Привязку permission → фича берёт из <see cref="PermissionRegistry"/>
/// (объявления <c>[Permission]</c> этого процесса), состояние фичи — у <see cref="IFeatureManager"/>.
/// <para>
/// Permission, не объявленный в этом процессе, реестру неизвестен — гейтить его нечем, проверка
/// идёт по одним claims (fail-open). Незаведённый фич-флаг, наоборот, считается выключенным
/// (fail-closed) — это семантика <see cref="IFeatureManager"/>.
/// </para>
/// </summary>
public sealed class ClaimPermissionAuthorizer : IPermissionAuthorizer
{
    private readonly PermissionRegistry _registry;
    private readonly IFeatureManager _features;

    public ClaimPermissionAuthorizer(PermissionRegistry registry, IFeatureManager features)
    {
        _registry = registry;
        _features = features;
    }

    public async ValueTask<bool> HasAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        if (!user.HasClaim(PermissionConstants.PermissionClaimType, permission)) return false;
        return await IsFeatureEnabledAsync(permission, ct);
    }

    public async ValueTask<bool> HasAllAsync(
        ClaimsPrincipal user, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true) return false;

        foreach (var permission in permissions)
            if (!await HasAsync(user, permission, ct))
                return false;

        return true;
    }

    public async ValueTask<bool> HasAnyAsync(
        ClaimsPrincipal user, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true) return false;

        foreach (var permission in permissions)
            if (await HasAsync(user, permission, ct))
                return true;

        return false;
    }

    // Фичу спрашиваем только у permission'ов, которые к ней привязаны — остальные не платят за проверку.
    private ValueTask<bool> IsFeatureEnabledAsync(string permission, CancellationToken ct)
    {
        var feature = _registry.GetFeature(permission);
        return feature is null ? ValueTask.FromResult(true) : _features.IsEnabledAsync(feature, ct: ct);
    }
}
