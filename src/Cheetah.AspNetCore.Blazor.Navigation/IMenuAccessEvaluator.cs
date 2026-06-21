using System.Security.Claims;

namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Решает, виден ли пользователю пункт меню с заданным требованием-разрешением.
/// Реализуется модулем Identity (поверх ролей/прав); Navigation остаётся независимым от Identity.
/// </summary>
public interface IMenuAccessEvaluator
{
    bool CanSee(ClaimsPrincipal user, string? requiredPermission);
}
