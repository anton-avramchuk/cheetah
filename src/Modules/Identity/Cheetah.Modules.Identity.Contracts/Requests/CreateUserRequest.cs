using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

/// <summary>
/// Базовый запрос создания пользователя. Реальное приложение наследует этот тип
/// конкретным record-ом (можно добавить поля) и подключает через
/// <c>AddCrmIdentity(...).WithUsers&lt;...&gt;()</c>.
/// </summary>
public abstract record CreateUserRequest(
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    [property: EmailAddress]
    string Email,
    [property: Required(AllowEmptyStrings = false)]
    string Password,
    IReadOnlyList<Guid>? RoleIds = null) : ICrmRequest;
